document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".like-btn").forEach(button => {
        button.addEventListener("click", function () {
            var itemId = this.getAttribute("data-id");
            var tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');

            if (!tokenElement) {
                console.error("RequestVerificationToken not found. Make sure you have a form with @Html.AntiForgeryToken() on the page.");
                return;
            }

            fetch("/Item/ToggleLike", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": tokenElement.value
                },
                body: JSON.stringify({ itemId: itemId })
            })
                .then(response => {
                    if (!response.ok) {
                        throw new Error("Network response was not ok");
                    }
                    return response.json();
                })
                .then(data => {
                    document.getElementById("like-count-" + itemId).innerText = data.likes;
                })
                .catch(error => console.error("Error toggling like:", error));
        });
    });
});
