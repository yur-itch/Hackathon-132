self.addEventListener("push", (event) => {
  let data = { title: "PlantCare", body: "У вас новое напоминание." };

  if (event.data) {
    try {
      data = event.data.json();
    } catch {
      data.body = event.data.text();
    }
  }

  event.waitUntil(
    self.registration.showNotification(data.title || "PlantCare", {
      body: data.body || "",
      icon: "/favicon.ico",
    }),
  );
});

self.addEventListener("notificationclick", (event) => {
  event.notification.close();
  event.waitUntil(
    self.clients.matchAll({ type: "window" }).then((clientsList) => {
      const existing = clientsList.find((c) => "focus" in c);
      if (existing) return existing.focus();
      return self.clients.openWindow("/reminders");
    }),
  );
});
