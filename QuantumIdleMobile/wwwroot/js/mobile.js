// 移动版通用 JavaScript

// API 基础配置
// 自动修复：如果本地存储了旧的 5000 端口地址，强制清除，使用相对路径
let savedUrl = localStorage.getItem('apiBaseUrl');
if (savedUrl && savedUrl.includes(':5000')) {
    console.log('Detected legacy API URL, clearing...');
    localStorage.removeItem('apiBaseUrl');
    savedUrl = null;
}

// API 基础配置
const API_CONFIG = {
    // 自动使用当前域名和协议，不再硬编码端口
    baseUrl: savedUrl || '/api',
    getToken: () => localStorage.getItem('token')
};

// 通用 API 请求函数
async function apiRequest(endpoint, options = {}) {
    const token = API_CONFIG.getToken();

    const defaultOptions = {
        headers: {
            'Content-Type': 'application/json',
            ...(token && { 'Authorization': `Bearer ${token}` })
        }
    };

    const response = await fetch(`${API_CONFIG.baseUrl}${endpoint}`, {
        ...defaultOptions,
        ...options,
        headers: {
            ...defaultOptions.headers,
            ...(options.headers || {})
        }
    });

    if (response.status === 401) {
        // Token 过期，跳转到登录页
        localStorage.removeItem('token');
        localStorage.removeItem('userInfo');
        window.location.href = '/Home/Login';
        return null;
    }

    return response.json();
}

// 显示提示消息
function showMessage(message, type = 'info') {
    // 简单的 alert 实现，可以后续替换为更好的 UI 组件
    alert(message);
}

// 格式化日期时间
function formatDateTime(dateString) {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleString('zh-CN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
    });
}

// 格式化金额
function formatAmount(amount) {
    if (amount === null || amount === undefined) return '0.00';
    return parseFloat(amount).toFixed(2);
}

// 检查登录状态
function checkAuth() {
    const token = localStorage.getItem('token');
    if (!token) {
        window.location.href = '/Home/Login';
        return false;
    }
    return true;
}

// 导出到全局
window.API_CONFIG = API_CONFIG;
window.apiRequest = apiRequest;
window.showMessage = showMessage;
window.formatDateTime = formatDateTime;
window.formatAmount = formatAmount;
window.checkAuth = checkAuth;

