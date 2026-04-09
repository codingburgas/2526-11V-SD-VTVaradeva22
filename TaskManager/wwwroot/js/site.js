(() => {
  const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
  const token = tokenInput ? tokenInput.value : null;

  const boardSelect = document.querySelector(".js-board-select");
  const listSelect = document.querySelector(".js-list-select");

  if (boardSelect && listSelect) {
    boardSelect.addEventListener("change", async () => {
      const boardId = boardSelect.value;
      listSelect.innerHTML = "";

      if (!boardId) {
        return;
      }

      const response = await fetch(`/Tasks/GetLists?boardId=${boardId}`);
      if (!response.ok) {
        return;
      }

      const lists = await response.json();
      for (const item of lists) {
        const option = document.createElement("option");
        option.value = item.value;
        option.textContent = item.text;
        listSelect.appendChild(option);
      }
    });
  }

  const cards = document.querySelectorAll(".kanban-card");
  const columns = document.querySelectorAll(".kanban-list-body");
  let draggedTaskId = null;

  cards.forEach(card => {
    card.addEventListener("dragstart", () => {
      draggedTaskId = card.dataset.taskId;
    });
  });

  columns.forEach(column => {
    column.addEventListener("dragover", event => {
      event.preventDefault();
      column.classList.add("drag-over");
    });

    column.addEventListener("dragleave", () => {
      column.classList.remove("drag-over");
    });

    column.addEventListener("drop", async event => {
      event.preventDefault();
      column.classList.remove("drag-over");

      if (!draggedTaskId) {
        return;
      }

      const targetListId = column.dataset.listId;
      const headers = {
        "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8"
      };

      if (token) {
        headers.RequestVerificationToken = token;
      }

      const response = await fetch("/Tasks/Move", {
        method: "POST",
        headers,
        body: new URLSearchParams({
          taskId: draggedTaskId,
          targetListId
        })
      });

      if (response.ok) {
        const draggedCard = document.querySelector(`[data-task-id="${draggedTaskId}"]`);
        if (draggedCard) {
          column.appendChild(draggedCard);
        }
      } else {
        alert("The task could not be moved.");
      }

      draggedTaskId = null;
    });
  });
})();
