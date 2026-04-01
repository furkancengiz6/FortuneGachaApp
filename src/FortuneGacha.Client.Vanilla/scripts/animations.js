const animations = {
    async playGachaOpening() {
        const orb = document.getElementById("gacha-orb");
        const btn = document.getElementById("draw-button");
        orb.classList.add("shake");
        btn.disabled = true;
        await new Promise(r => setTimeout(r, 1500));
        orb.classList.remove("shake");
    },

    async playAILoading(show) {
        const overlay = document.getElementById("ai-loading-overlay");
        const status = document.getElementById("ai-loading-status");
        const messages = ["Yıldızlar hizalanıyor...", "Kaderin dokunuyor...", "Gelecek şekilleniyor...", "Kadim bilgiler derleniyor..."];
        
        if (show) {
            overlay.classList.remove("hidden");
            overlay.classList.add("flex");
            let i = 0;
            this.msgInterval = setInterval(() => {
                status.innerText = messages[++i % messages.length];
            }, 3000);
        } else {
            clearInterval(this.msgInterval);
            overlay.classList.add("hidden");
            overlay.classList.remove("flex");
        }
    },

    showFortuneModal(fortune) {
        const modal = document.getElementById("fortune-modal");
        const img = document.getElementById("modal-image");
        const text = document.getElementById("modal-text");
        const badge = document.getElementById("modal-rarity-badge");

        img.src = fortune.imageUrl;
        text.innerText = fortune.fortuneText;
        badge.innerText = fortune.rarity;
        
        // Rarity Styling
        badge.className = `absolute top-6 left-6 px-4 py-1.5 rounded-full text-xs font-bold tracking-widest uppercase shadow-lg`;
        if (fortune.rarity === "Legendary") {
            badge.classList.add("bg-amber-500", "text-black", "gold-glow");
            modal.querySelector('.glass').className = "glass max-w-lg w-full rounded-[2.5rem] relative z-[101] overflow-hidden rarity-Legendary";
        } else if (fortune.rarity === "Rare") {
            badge.classList.add("bg-purple-600", "text-white");
            modal.querySelector('.glass').className = "glass max-w-lg w-full rounded-[2.5rem] relative z-[101] overflow-hidden rarity-Rare";
        } else {
            badge.classList.add("bg-gray-700", "text-gray-300");
            modal.querySelector('.glass').className = "glass max-w-lg w-full rounded-[2.5rem] relative z-[101] overflow-hidden rarity-Common";
        }

        modal.classList.remove("hidden");
        modal.classList.add("flex");
    }
};
