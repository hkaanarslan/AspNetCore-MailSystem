document.addEventListener("DOMContentLoaded", function () {

    const checkAll = document.getElementById("checkAll");
    const searchInput = document.getElementById("searchInput");
    const btnRefresh = document.getElementById("btnRefresh");
    const btnDelete = document.getElementById("btnDelete");

    /* ================= SELECT ALL ================= */
    if (checkAll && !checkAll.dataset.bound) {
        checkAll.dataset.bound = "true";

        checkAll.addEventListener("change", e => {
            document.querySelectorAll(".mail-check")
                .forEach(c => c.checked = e.target.checked);
        });
    }


    /* ================= SEARCH ================= */
    if (searchInput && !searchInput.dataset.bound) {
        searchInput.dataset.bound = "true";

        searchInput.addEventListener("keyup", function () {
            const val = this.value.toLowerCase();

            document.querySelectorAll(".mail-row").forEach(row => {
                row.style.display =
                    row.innerText.toLowerCase().includes(val) ? "" : "none";
            });
        });
    }


    /* ================= STAR (AJAX) ================= */
    document.querySelectorAll(".star-btn").forEach(star => {

        if (star.dataset.bound) return;
        star.dataset.bound = "true";

        star.addEventListener("click", async function (e) {
            e.stopPropagation();

            const row = this.closest(".mail-row");
            const id = row?.dataset?.id;
            if (!id) return;

            try {
                const res = await fetch(`/Message/ToggleStar/${id}`, {
                    method: "POST"
                });

                if (res.ok) {
                    this.classList.toggle("bxs-star");
                    this.classList.toggle("bx-star");
                    this.classList.toggle("star-active");
                }
            } catch (err) {
                console.error("Star hatası:", err);
            }
        });
    });


    /* ================= ROW CLICK → DETAIL ================= */
    document.querySelectorAll(".mail-row").forEach(row => {

        if (row.dataset.bound) return;
        row.dataset.bound = "true";

        row.addEventListener("click", function () {

            const id = this.dataset.id;
            if (!id) return;

            window.location.href = "/Message/MessageDetail/" + id;
        });
    });


    /* ================= REFRESH ================= */
    if (btnRefresh && !btnRefresh.dataset.bound) {
        btnRefresh.dataset.bound = "true";
        btnRefresh.addEventListener("click", () => location.reload());
    }


    /* ================= MULTI DELETE (AJAX) ================= */
    if (btnDelete && !btnDelete.dataset.bound) {

        btnDelete.dataset.bound = "true";

        btnDelete.addEventListener("click", async () => {

            const selected = Array.from(document.querySelectorAll(".mail-check:checked"))
                .map(x => parseInt(x.value));

            if (selected.length === 0) {
                alert("Lütfen silinecek mesajları seç.");
                return;
            }

            if (!confirm("Seçili mesajlar silinsin mi?"))
                return;

            try {
                const res = await fetch("/Message/DeleteMultiple", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(selected)
                });

                if (res.ok) {
                    selected.forEach(id => {
                        document.querySelector(`.mail-row[data-id='${id}']`)?.remove();
                    });
                } else {
                    alert("Silme sırasında hata oluştu.");
                }
            } catch (err) {
                console.error("Delete hatası:", err);
                alert("Silme sırasında hata oluştu.");
            }
        });
    }

});