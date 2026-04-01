document.addEventListener('DOMContentLoaded', initApp);

let currentUser = { username: '', gp: 0 };

function initApp() {
    checkAuthState();
    setupEventListeners();
    if (localStorage.getItem("token")) {
        loadUserData();
        loadShowcase();
        loadSocialData();
    }
}

async function loadUserData() {
    try {
        const user = await api.auth.me();
        document.getElementById("current-username").innerText = `@${user.username}`;
        updateGPDisplay(user.gachaPoints);
        currentUser = { username: user.username, gp: user.gachaPoints };
        
        if (user.lastDrawDate && new Date(user.lastDrawDate).toDateString() === new Date().toDateString()) {
            document.getElementById("draw-button").disabled = true;
            document.getElementById("limit-msg").classList.remove("hidden");
        }
    } catch (err) { console.error(err); }
}

function checkAuthState() {
    const token = localStorage.getItem("token");
    if (token) {
        document.getElementById("auth-nav").classList.add("hidden");
        document.getElementById("user-nav").classList.remove("hidden");
        document.getElementById("draw-button").disabled = false;
    } else {
        document.getElementById("auth-nav").classList.remove("hidden");
        document.getElementById("user-nav").classList.add("hidden");
        document.getElementById("draw-button").disabled = true;
    }
}

function setupEventListeners() {
    // Auth
    document.getElementById("nav-login").addEventListener("click", () => showAuthModal("Login"));
    document.getElementById("nav-register").addEventListener("click", () => showAuthModal("Register"));
    document.getElementById("auth-switch").addEventListener("click", switchAuthMode);
    document.getElementById("auth-submit").addEventListener("click", handleAuthSubmit);
    document.getElementById("logout-btn").addEventListener("click", () => { localStorage.clear(); location.reload(); });
    document.getElementById("close-auth").addEventListener("click", () => document.getElementById("auth-modal").classList.add("hidden"));

    // Gacha
    document.getElementById("draw-button").addEventListener("click", handleGachaDraw);
    document.getElementById("close-modal").addEventListener("click", () => {
        animations.hideFortuneModal();
        loadShowcase();
        loadSocialData(); // GP updates
    });

    // Social Toggle
    document.getElementById("social-toggle").addEventListener("click", toggleSocialPanel);
    document.getElementById("close-social").addEventListener("click", toggleSocialPanel);
    
    // Search
    document.getElementById("user-search").addEventListener("input", debounce(handleUserSearch, 500));
}

function toggleSocialPanel() {
    const panel = document.getElementById("social-panel");
    panel.classList.toggle("translate-x-full");
}

function showAuthModal(mode) {
    const modal = document.getElementById("auth-modal");
    modal.classList.remove("hidden");
    const title = document.getElementById("auth-title");
    const email = document.getElementById("auth-email");
    if (mode === "Login") {
        title.innerText = "Giriş Yap"; email.classList.add("hidden");
    } else {
        title.innerText = "Kayıt Ol"; email.classList.remove("hidden");
    }
}

function switchAuthMode() {
    const mode = document.getElementById("auth-title").innerText === "Giriş Yap" ? "Register" : "Login";
    showAuthModal(mode);
}

async function handleAuthSubmit() {
    const mode = document.getElementById("auth-title").innerText;
    const u = document.getElementById("auth-username").value;
    const p = document.getElementById("auth-password").value;
    const e = document.getElementById("auth-email").value;

    try {
        const res = (mode === "Giriş Yap") ? await api.auth.login(u, p) : await api.auth.register(u, e, p);
        localStorage.setItem("token", res.token);
        localStorage.setItem("username", res.username);
        location.reload();
    } catch (err) { alert(err.message); }
}

async function handleGachaDraw() {
    const boost = document.getElementById("luck-boost").checked;
    try {
        await animations.playGachaOpening();
        animations.playAILoading(true);
        
        const res = await api.fortune.draw(boost);
        
        setTimeout(() => {
            animations.playAILoading(false);
            animations.showFortuneModal(res);
            updateGPDisplay(res.gachaPoints);
        }, 3000);
    } catch (err) {
        animations.playAILoading(false);
        alert(err.message);
    }
}

async function loadShowcase(targetUser = null) {
    const username = targetUser || localStorage.getItem("username");
    document.getElementById("vitrin-owner").innerText = targetUser ? `@${targetUser} Vitrini` : "Kendi kehanetlerin.";
    
    try {
        const fortunes = await api.fortune.getShowcase(username);
        const grid = document.getElementById("fortune-grid");
        grid.innerHTML = fortunes.map(f => `
            <div class="glass rounded-3xl overflow-hidden group rarity-${f.rarity} transition-all duration-500">
                <div class="h-64 relative">
                    <img src="${f.imageUrl}" class="w-full h-full object-cover group-hover:scale-110 transition-transform duration-700">
                    <div class="absolute inset-0 bg-gradient-to-t from-black/80 to-transparent flex items-end p-6">
                        <p class="text-sm font-medium line-clamp-2">${f.fortuneText}</p>
                    </div>
                    <div class="absolute top-4 left-4 text-[10px] bg-black/50 px-2 py-0.5 rounded-full border border-white/10 uppercase tracking-widest font-bold">${f.rarity}</div>
                </div>
                <div class="p-4 flex justify-between items-center text-xs text-gray-500">
                    <span>${new Date(f.drawDate).toLocaleDateString('tr-TR')}</span>
                    <button class="flex items-center gap-1 hover:text-red-400 transition" onclick="likeFortune(${f.id}, this)">
                        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z"></path></svg>
                        <span>${f.likeCount || 0}</span>
                    </button>
                </div>
            </div>
        `).join('');
    } catch (err) { console.error(err); }
}

async function likeFortune(id, btn) {
    try {
        await api.social.like(id);
        btn.querySelector('span').innerText = parseInt(btn.querySelector('span').innerText) + 1;
        loadSocialData(); // Refresh GP
    } catch (err) { alert(err.message); }
}

async function loadSocialData() {
    try {
        const friends = await api.social.getFriends();
        const pending = await api.social.getPendingRequests();
        const gpDisplay = document.getElementById("user-gp");

        // Update GP from friend list data (backend sends it as a shortcut)
        // Note: Real solution is a separate /me endpoint
        const currentFriend = friends.find(f => f.username === currentUser.username);
        // if (currentFriend) gpDisplay.innerText = currentFriend.gachaPoints;

        document.getElementById("req-count").innerText = pending.length;
        document.getElementById("req-count").classList.toggle("hidden", pending.length === 0);

        renderFriends(friends);
        window.pendingRequests = pending; // Store for tab switching if needed
    } catch (err) { console.error(err); }
}

function renderFriends(friends) {
    const list = document.getElementById("social-list");
    list.innerHTML = friends.map(f => `
        <div class="flex justify-between items-center bg-white/5 p-3 rounded-xl hover:bg-white/10 cursor-pointer transition" onclick="loadShowcase('${f.username}')">
            <div class="flex items-center gap-3">
                <div class="w-8 h-8 rounded-full bg-gradient-to-br from-indigo-500 to-purple-500"></div>
                <span class="font-medium">@${f.username}</span>
            </div>
            <span class="text-[10px] text-amber-500 font-bold">${f.gachaPoints} GP</span>
        </div>
    `).join('');
}

async function handleUserSearch(e) {
    const q = e.target.value;
    if (q.length < 2) { document.getElementById("search-results").innerHTML = ""; return; }
    try {
        const results = await api.social.search(q);
        document.getElementById("search-results").innerHTML = results.map(u => `
            <div class="flex justify-between items-center p-2 bg-white/5 rounded-lg">
                <span class="text-sm">@${u.username}</span>
                <button class="text-xs bg-purple-600 px-2 py-1 rounded" onclick="sendFriendRequest(${u.id})">Ekle</button>
            </div>
        `).join('');
    } catch (err) { console.error(err); }
}

async function sendFriendRequest(userId) {
    try {
        await api.social.sendRequest(userId);
        alert("İstek gönderildi.");
        document.getElementById("user-search").value = "";
        document.getElementById("search-results").innerHTML = "";
    } catch (err) { alert(err.message); }
}

function updateGPDisplay(gp) {
    document.getElementById("user-gp").innerText = gp;
}

function debounce(func, wait) {
    let timeout;
    return function(...args) {
        clearTimeout(timeout);
        timeout = setTimeout(() => func.apply(this, args), wait);
    };
}

// Global functions for inline onclicks
window.likeFortune = likeFortune;
window.sendFriendRequest = sendFriendRequest;
window.loadShowcase = loadShowcase;
