const API_BASE = "http://localhost:5000/api";

const api = {
    async request(endpoint, method = "GET", body = null, params = {}) {
        const token = localStorage.getItem("token");
        const headers = { "Content-Type": "application/json" };
        if (token) headers["Authorization"] = `Bearer ${token}`;

        let url = `${API_BASE}${endpoint}`;
        const queryParams = new URLSearchParams(params).toString();
        if (queryParams) url += `?${queryParams}`;

        const config = { method, headers };
        if (body) config.body = JSON.stringify(body);

        try {
            const response = await fetch(url, config);
            if (!response.ok) {
                const error = await response.text();
                throw new Error(error || response.statusText);
            }
            return await response.json();
        } catch (err) {
            console.error("API Error:", err);
            throw err;
        }
    },

    auth: {
        async login(username, password) { return api.request("/auth/login", "POST", { username, password }); },
        async register(username, email, password) { return api.request("/auth/register", "POST", { username, email, password }); },
        async me() { return api.request("/auth/me", "GET"); }
    },

    fortune: {
        async draw(boost = false) { return api.request("/fortune/draw", "POST", null, { boost }); },
        async getShowcase(username) { return api.request(`/fortune/showcase/${username}`); },
        async getMyFortunes() { return api.request("/fortune/my-fortunes"); }
    },

    social: {
        async like(id) { return api.request(`/social/like/${id}`, "POST"); },
        async search(q) { return api.request("/social/search", "GET", null, { q }); },
        async sendRequest(userId) { return api.request(`/social/request/${userId}`, "POST"); },
        async respondRequest(id, accept) { return api.request(`/social/respond/${id}`, "POST", null, { accept }); },
        async getFriends() { return api.request("/social/friends"); },
        async getPendingRequests() { return api.request("/social/requests/pending"); }
    }
};
