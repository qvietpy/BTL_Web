// --- LOGIC BACK TO TOP ---
let mybutton = document.getElementById("btn-back-to-top");

window.onscroll = function () {
    scrollFunction();
};

function scrollFunction() {
    if (document.body.scrollTop > 20 || document.documentElement.scrollTop > 20) {
        mybutton.style.display = "block";
    } else {
        mybutton.style.display = "none";
    }
}

mybutton.addEventListener("click", backToTop);

function backToTop() {
    document.body.scrollTop = 0;
    document.documentElement.scrollTop = 0;
}

function toggleText(elementId, btn) {
    var content = document.getElementById(elementId);
    if (!content) return;
    if (content.classList.contains("text-truncate-3")) {
        content.classList.remove("text-truncate-3");
        content.classList.add("show");
        btn.innerHTML = "Thu gọn";
    } else {
        content.classList.add("text-truncate-3");
        content.classList.remove("show");
        btn.innerHTML = "Xem thêm";
    }
}