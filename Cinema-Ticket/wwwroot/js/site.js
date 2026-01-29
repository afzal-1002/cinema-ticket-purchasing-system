// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// global confirmation handler for elements using data-confirm
document.addEventListener('DOMContentLoaded', function () {
	var elementToAct = null;
	var confirmMessageElem = document.getElementById('globalConfirmMessage');
	var confirmOkBtn = document.getElementById('globalConfirmOk');

	document.body.addEventListener('click', function (e) {
		var t = e.target;
		// find nearest element with data-confirm attribute (walk up)
		while (t && t !== document.body) {
			if (t.dataset && t.dataset.confirm) {
				e.preventDefault();
				elementToAct = t;
				if (confirmMessageElem) confirmMessageElem.textContent = t.dataset.confirm;
				var modalEl = document.getElementById('globalConfirmModal');
				var modal = new bootstrap.Modal(modalEl);
				modal.show();
				return;
			}
			t = t.parentElement;
		}
	});

	if (confirmOkBtn) {
		confirmOkBtn.addEventListener('click', function () {
			if (!elementToAct) return;

			// if inside a form, submit the form
			var form = elementToAct.closest('form');
			if (form) {
				form.submit();
				return;
			}

			// otherwise, if it's a link, follow it
			if (elementToAct.tagName === 'A' && elementToAct.href) {
				window.location.href = elementToAct.href;
				return;
			}

			// fallback: click the element
			elementToAct.click();
		});
	}
    // (ajax modal removed) — keep only confirmation handler above
});
