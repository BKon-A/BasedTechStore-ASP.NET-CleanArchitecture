document.addEventListener("DOMContentLoaded", function () {
    const categoryLinks = document.querySelectorAll(".category-link");

    categoryLinks.forEach(link => {
        link.addEventListener("click", function (e) {
            e.preventDefault();

            const category = this.dataset.category;
            const submenu = document.getElementById(`${category}Submenu`);

            document.querySelectorAll(".submenu").forEach(sub => {
                if (sub !== submenu) {
                    sub.style.display = "none";
                }
            });

            submenu.style.display = submenu.style.display === "block" ? "none" : "block";
        });
    });
});