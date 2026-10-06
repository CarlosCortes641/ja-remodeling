(() => {
  const locale = document.documentElement.lang === "es" ? "es" : "en";

  const copy = {
    en: {
      sending: "Sending…",
      success: "Request received",
      successBody: "Save this reference. J&A can use it to locate your intake:",
      retry: "Please try again or call 980-298-4600.",
      plannerSuccess: "Planning intake received. Reference:",
      plannerError: "The request could not be sent. Please review required fields or call 980-298-4600."
    },
    es: {
      sending: "Enviando…",
      success: "Solicitud recibida",
      successBody: "Guarde esta referencia. J&A puede usarla para encontrar su solicitud:",
      retry: "Inténtelo nuevamente o llame al 980-298-4600.",
      plannerSuccess: "Solicitud recibida. Referencia:",
      plannerError: "No se pudo enviar. Revise los campos obligatorios o llame al 980-298-4600."
    }
  }[locale];

  const el = (tag, props = {}, children = []) => {
    const node = Object.assign(document.createElement(tag), props);
    node.append(...children);
    return node;
  };

  const postLead = async (payload) => {
    const response = await fetch("/api/leads", {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify(payload)
    });
    const data = await response.json().catch(() => ({}));
    if (!response.ok) throw new Error(data.error || "");
    return data.reference || "";
  };

  const setSending = (button, sending) => {
    const label = button.firstChild;
    if (sending) {
      button.dataset.label = label.textContent;
      label.textContent = copy.sending;
    } else if (button.dataset.label) {
      label.textContent = button.dataset.label;
    }
    button.disabled = sending;
  };

  document.querySelectorAll("form[data-lead-form]").forEach((form) => {
    form.addEventListener("submit", async (event) => {
      event.preventDefault();
      const button = form.querySelector("button[type=submit]");
      form.querySelector(".formError")?.remove();
      setSending(button, true);

      const data = new FormData(form);
      const payload = {
        ...Object.fromEntries(data.entries()),
        details: JSON.stringify({
          description: data.get("description"),
          projectGoal: data.get("projectGoal"),
          occupancy: data.get("occupancy"),
          budgetRange: data.get("budgetRange"),
          preferredContact: data.get("preferredContact")
        }),
        services: data.getAll("services").join(", "),
        locale,
        source: form.dataset.source || "homepage",
        consent: data.get("consent") === "on"
      };

      try {
        const reference = await postLead(payload);
        form.replaceWith(
          el("div", { className: "formSuccess", role: "status" }, [
            el("span", { textContent: "✓" }),
            el("h3", { textContent: copy.success }),
            el("p", { textContent: copy.successBody }),
            el("strong", { textContent: reference }),
            el("a", { href: "tel:+19802984600", textContent: "980-298-4600" })
          ])
        );
      } catch (error) {
        setSending(button, false);
        const message = [error.message, copy.retry].filter(Boolean).join(" ");
        button.after(el("p", { className: "formError", role: "alert", textContent: message }));
      }
    });
  });

  document.querySelectorAll("form[data-turn-planner]").forEach((form) => {
    // Required fields live in step 1; open their <details> so the browser can focus them.
    form.addEventListener(
      "invalid",
      (event) => {
        const details = event.target.closest("details");
        if (details) details.open = true;
      },
      true
    );

    form.querySelectorAll("a.stepAction[href^='#step-']").forEach((link) => {
      link.addEventListener("click", (event) => {
        const target = document.querySelector(link.getAttribute("href"));
        if (!target) return;
        event.preventDefault();
        target.open = true;
        target.scrollIntoView({ behavior: "smooth", block: "start" });
      });
    });

    form.addEventListener("submit", async (event) => {
      event.preventDefault();
      const button = form.querySelector("button[type=submit]");
      form.querySelectorAll(".plannerStatus").forEach((n) => n.remove());
      setSending(button, true);

      const data = new FormData(form);
      const payload = {
        locale,
        buyerType: "guided-planner",
        propertyAddress: data.get("step1_0"),
        company: data.get("step1_1"),
        unitCount: data.get("step1_2"),
        vacancyDate: data.get("step1_3"),
        targetDate: data.get("step1_4"),
        name: data.get("step1_5"),
        email: data.get("step1_6"),
        phone: data.get("step1_7"),
        details: Object.fromEntries(data.entries()),
        consent: data.get("consent") === "on",
        source: "six-step-planner"
      };

      try {
        const reference = await postLead(payload);
        setSending(button, false);
        form.reset();
        button.after(
          el("p", { className: "plannerStatus success" }, [
            `${copy.plannerSuccess} `,
            el("strong", { textContent: reference })
          ])
        );
      } catch {
        setSending(button, false);
        button.after(el("p", { className: "plannerStatus error", textContent: copy.plannerError }));
      }
    });
  });
})();
