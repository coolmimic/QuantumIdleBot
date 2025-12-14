using QuantumIdleDesktop.Models;
using QuantumIdleDesktop.Models.DrawRules;
using QuantumIdleDesktop.Models.Odds;
using QuantumIdleDesktop.Services;
using QuantumIdleDesktop.Strategies;
using QuantumIdleDesktop.Strategies.DrawRules;
using QuantumIdleDesktop.Utils;
using QuantumIdleDesktop.Views.Base;
using QuantumIdleDesktop.Views.DrawRules;
using QuantumIdleDesktop.Views.Odds;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TL; // Telegram 库引用

namespace QuantumIdleDesktop.Forms
{
    // 这里的 partial 与 UI.cs 合并为一个类
    public partial class FormSchemeEditor : Form
    {
        public SchemeModel ResultScheme { get; private set; }
        private readonly string _editingId;

        private OddsEditorBase _oddsEditor;
        private DrawRuleEditorBase _ruleEditor;
        private List<dynamic> _fullGroupList = new List<dynamic>();

        public FormSchemeEditor()
        {
            // 1. 调用手写的 UI 初始化方法
            InitUI();

            // 2. 初始化业务逻辑
            InitLogic();
        }

        public FormSchemeEditor(SchemeModel scheme) : this()
        {
            _editingId = scheme?.Id;
            // 3. 如果是编辑模式，加载数据
            LoadScheme(scheme);
        }

        /// <summary>
        /// 初始化业务逻辑（事件绑定、默认值等）
        /// </summary>
        private void InitLogic()
        {
            // 设置搜索框占位符
            SetPlaceholder(txtGroupSearch, "输入关键词搜索...");

            // 绑定枚举到下拉框
            cboGameType.BindEnum<GameType>();
            cboOddsType.BindEnum<OddsType>();
            cboRuleType.BindEnum<DrawRuleType>();

            // --- 事件绑定 ---

            // 游戏类型改变 -> 刷新玩法列表
            cboGameType.SelectedIndexChanged += (s, e) => BindPlayModes();

            // 玩法改变 -> 检查是否显示位置面板 (万千百十个)
            cboPlayMode.SelectedIndexChanged += (s, e) => UpdatePositionPanelVisibility();

            // 策略类型改变 -> 加载对应的编辑器
            cboOddsType.SelectedIndexChanged += (s, e) => LoadOddsEditor();
            cboRuleType.SelectedIndexChanged += (s, e) => LoadRuleEditor();

            // 群组相关
            btnRefreshGroup.Click += async (s, e) => await RefreshGroupsAsync();
            txtGroupSearch.TextChanged += TxtGroupSearch_TextChanged;

            // 保存与取消
            btnSave.Click += btnSave_Click;
            btnCancel.Click += (s, e) => Close();

            // 初始加载群组缓存
            LoadGroupsFromCache();

            // --- 新建模式默认设置 ---
            if (string.IsNullOrEmpty(_editingId))
            {
                cboOddsType.SelectedValue = OddsType.Linear;
                cboRuleType.SelectedValue = DrawRuleType.Fixed;

                var nextNum = (CacheData.Schemes?.Count ?? 0) + 1;
                txtName.Text = $"方案{nextNum}";

                LoadOddsEditor();
                LoadRuleEditor();
            }
        }

        // ==================== 核心逻辑：玩法与位置 ====================

        private void BindPlayModes()
        {
            var selectedGame = cboGameType.GetSelectedEnum<GameType>();
            // 防止初始化时的无效调用
            if (!Enum.IsDefined(typeof(GameType), selectedGame)) return;

            // 1. 从工厂获取策略，询问支持的玩法
            var strategy = GameStrategyFactory.GetStrategy(selectedGame);
            if (strategy == null)
            {
                MessageBox.Show("该游戏还未更新");
                return;
            }
            var modes = strategy.GetSupportedModes();

            // 2. 构造数据源
            var dataSource = modes.Select(m => new
            {
                Name = EnumHelper.GetDescription(m),
                Value = m
            }).ToList();

            // 3. 记录旧值并重新绑定
            var oldVal = cboPlayMode.SelectedValue;

            cboPlayMode.DataSource = null;
            cboPlayMode.DisplayMember = "Name";
            cboPlayMode.ValueMember = "Value";
            cboPlayMode.DataSource = dataSource;

            // 4. 尝试恢复选中
            if (oldVal != null && dataSource.Any(x => x.Value.Equals(oldVal)))
                cboPlayMode.SelectedValue = oldVal;
            else if (dataSource.Count > 0)
                cboPlayMode.SelectedIndex = 0;

            // 5. 绑定完立刻更新位置面板状态
            UpdatePositionPanelVisibility();
        }

        private void UpdatePositionPanelVisibility()
        {
            var gameType = cboGameType.GetSelectedEnum<GameType>();
            var playMode = cboPlayMode.GetSelectedEnum<GamePlayMode>();

            // 判断条件：必须是 HashLottery 且 玩法需要位置
            bool needPosition = gameType == GameType.HashLottery && IsPositionMode(playMode);

            // 控制面板显隐
            pnlPositions.Visible = needPosition;
        }

        private bool IsPositionMode(GamePlayMode mode)
        {
            return mode == GamePlayMode.PositionDigit ||
                   mode == GamePlayMode.PositionBigSmallOddEven ||
                   mode == GamePlayMode.DragonTiger ||
                   mode == GamePlayMode.Sum;
        }

        // ==================== 数据加载与保存 ====================

        private void LoadScheme(SchemeModel s)
        {
            this.Text = $"编辑方案 - {s.Name}";
            txtName.Text = s.Name;

            // 1. 先选游戏类型 (会触发 BindPlayModes)
            cboGameType.SelectedValue = s.GameType;
            // 2. 再选玩法 (会触发 UpdatePositionPanelVisibility)
            cboPlayMode.SelectedValue = s.PlayMode;

            // 3. 回显群组
            txtGroupSearch.Text = "";
            FilterGroupList("");
            cboGroup.SelectedValue = s.TgGroupId;
            if (cboGroup.SelectedIndex == -1) cboGroup.Text = s.TgGroupName;

            // 4. 回显止盈止损
            chkRisk.Checked = s.EnableStopProfitLoss;
            numStopProfit.Value = Math.Min(numStopProfit.Maximum, s.StopProfitAmount);
            numStopLoss.Value = Math.Min(numStopLoss.Maximum, s.StopLossAmount);

            // 5. 回显策略编辑器
            cboOddsType.SelectedValue = s.OddsType;
            cboRuleType.SelectedValue = s.DrawRule;
            _oddsEditor?.SetConfiguration(s.OddsConfig);
            _ruleEditor?.SetConfiguration(s.DrawRuleConfig);

            // 6. 【关键】回显位置选择 (万千百十个)
            if (s.PositionLst != null && _posCheckBoxes != null)
            {
                foreach (var chk in _posCheckBoxes)
                {
                    // Tag 在 InitUI 里已经存了 int 索引 (0-4)
                    int idx = (int)chk.Tag;
                    chk.Checked = s.PositionLst.Contains(idx);
                }
            }

            // 再次刷新面板显示状态，确保万无一失
            UpdatePositionPanelVisibility();
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // --- 基础校验 ---
                if (string.IsNullOrWhiteSpace(txtName.Text)) throw new Exception("请输入方案名称");
                if (cboGroup.SelectedValue == null) throw new Exception("请选择群组");

                var gameType = cboGameType.GetSelectedEnum<GameType>();
                var playMode = cboPlayMode.GetSelectedEnum<GamePlayMode>();

                // --- 【新增】位置校验逻辑 ---
                List<int> selectedPositions = new List<int>();

                // 只有 HashLottery 且特定玩法才校验位置
                if (gameType == GameType.HashLottery && IsPositionMode(playMode))
                {
                    selectedPositions = _posCheckBoxes
                        .Where(c => c.Checked)
                        .Select(c => (int)c.Tag) // 获取索引 0-4
                        .OrderBy(x => x)
                        .ToList();

                    // 校验规则
                    if ((playMode == GamePlayMode.PositionDigit || playMode == GamePlayMode.PositionBigSmallOddEven)
                        && selectedPositions.Count < 1)
                    {
                        throw new Exception("当前玩法至少需要选择 1 个位置（万/千/百/十/个）");
                    }

                    if ((playMode == GamePlayMode.DragonTiger || playMode == GamePlayMode.Sum)
                        && selectedPositions.Count < 2)
                    {
                        throw new Exception("该玩法（龙虎/和值）至少需要选择 2 个位置");
                    }

                    if (playMode == GamePlayMode.DragonTiger && selectedPositions.Count != 2)
                    {
                        throw new Exception("龙虎斗必须严格选择 2 个位置（例如：万 vs 个）");
                    }
                }

                // --- 获取编辑器配置 ---
                var ruleConfig = _ruleEditor?.GetConfiguration();
                var oddsConfig = _oddsEditor?.GetConfiguration();
                if (ruleConfig == null) throw new Exception("规则配置无效");
                if (oddsConfig == null) throw new Exception("资金策略配置无效");

                // --- 构建模型 ---
                ResultScheme = new SchemeModel
                {
                    Id = _editingId ?? Guid.NewGuid().ToString(),
                    Name = txtName.Text.Trim(),
                    GameType = gameType,
                    PlayMode = playMode,

                    // 保存位置列表
                    PositionLst = selectedPositions,

                    TgGroupName = cboGroup.Text,
                    TgGroupId = Convert.ToInt64(cboGroup.SelectedValue),

                    OddsType = cboOddsType.GetSelectedEnum<OddsType>(),
                    DrawRule = cboRuleType.GetSelectedEnum<DrawRuleType>(),
                    DrawRuleConfig = ruleConfig,
                    OddsConfig = oddsConfig,

                    EnableStopProfitLoss = chkRisk.Checked,
                    StopProfitAmount = numStopProfit.Value,
                    StopLossAmount = numStopLoss.Value
                };

                // --- 保存到缓存文件 ---
                var list = CacheData.Schemes ??= new();
                var idx = list.FindIndex(x => x.Id == _editingId);
                if (idx >= 0) list[idx] = ResultScheme;
                else list.Add(ResultScheme);

                await SchemeFileHelper.SaveListAsync(list);

                this.DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "保存失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ==================== 辅助逻辑 (群组搜索/动态加载编辑器) ====================

        private void SetPlaceholder(TextBox txt, string placeholder)
        {
            txt.Text = placeholder;
            txt.ForeColor = Color.Gray;

            txt.Enter += (s, e) => {
                if (txt.Text == placeholder) { txt.Text = ""; txt.ForeColor = Color.White; }
            };
            txt.Leave += (s, e) => {
                if (string.IsNullOrWhiteSpace(txt.Text)) { txt.Text = placeholder; txt.ForeColor = Color.Gray; FilterGroupList(""); }
            };
        }

        private void TxtGroupSearch_TextChanged(object sender, EventArgs e)
        {
            var keyword = txtGroupSearch.Text.Trim();
            if (keyword == "输入关键词搜索...") return;
            FilterGroupList(keyword);
        }

        private void FilterGroupList(string keyword)
        {
            if (_fullGroupList == null || !_fullGroupList.Any()) return;

            var filtered = string.IsNullOrEmpty(keyword)
                ? _fullGroupList
                : _fullGroupList.Where(g => g.Name != null && g.Name.ToLower().Contains(keyword.ToLower())).ToList();

            var oldId = cboGroup.SelectedValue;
            cboGroup.DataSource = null;
            cboGroup.DataSource = filtered;
            cboGroup.DisplayMember = "Name";
            cboGroup.ValueMember = "Id";

            if (oldId != null && filtered.Any(x => x.Id.ToString() == oldId.ToString()))
                cboGroup.SelectedValue = oldId;
        }

        private void LoadGroupsFromCache()
        {
            if (CacheData.GroupLst?.Any() == true)
            {
                _fullGroupList = CacheData.GroupLst.ToList<dynamic>();
                FilterGroupList("");
            }
        }

        private async Task RefreshGroupsAsync()
        {
            this.Enabled = false;
            btnRefreshGroup.Text = "...";
            try
            {
                var list = await CacheData.tgService.GetAllChats();
                CacheData.GroupLst = list?.ToList() ?? new();
                LoadGroupsFromCache();
                MessageBox.Show($"群组已更新，共 {CacheData.GroupLst.Count} 个", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show("刷新失败: " + ex.Message); }
            finally { this.Enabled = true; btnRefreshGroup.Text = "刷新"; }
        }

        private void LoadOddsEditor()
        {
            pnlOddsContainer.Controls.Clear();
            var type = cboOddsType.GetSelectedEnum<OddsType>();
            _oddsEditor = type == OddsType.Linear ? new LinearOddsEditor() : null;

            if (_oddsEditor != null)
            {
                _oddsEditor.Dock = DockStyle.Top;
                pnlOddsContainer.Controls.Add(_oddsEditor);
            }
        }

        private void LoadRuleEditor()
        {
            pnlRuleContainer.Controls.Clear();
            var type = cboRuleType.GetSelectedEnum<DrawRuleType>();
            _ruleEditor = type switch
            {
                DrawRuleType.Fixed => new FixedRuleEditor(),
                DrawRuleType.FollowLast => new FollowLastRuleEditor(),
                DrawRuleType.SlayDragonFollowDragon => new SlayDragonFollowDragonEditor(),
                DrawRuleType.NumberTrend => new NumberTrendEditor(),
                DrawRuleType.PatternTrend => new PatternTrendEditor(),
                DrawRuleType.BranchTrend => new BranchTrendEditor(),
                DrawRuleType.ResultFollow => new ResultFollowEditor(),
                _ => null
            };

            if (_ruleEditor != null)
            {
                _ruleEditor.Dock = DockStyle.Top;
                pnlRuleContainer.Controls.Add(_ruleEditor);
            }
        }
    }
}