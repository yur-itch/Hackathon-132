import { api } from "./api/client.js";

function urlBase64ToUint8Array(base64String) {
  const padding = "=".repeat((4 - (base64String.length % 4)) % 4);
  const base64 = (base64String + padding).replace(/-/g, "+").replace(/_/g, "/");
  const rawData = atob(base64);
  return Uint8Array.from([...rawData].map((char) => char.charCodeAt(0)));
}

export function isPushSupported() {
  return "serviceWorker" in navigator && "PushManager" in window;
}

export async function getSubscriptionState() {
  if (!isPushSupported()) return { supported: false, subscribed: false };

  const registration = await navigator.serviceWorker.getRegistration();
  const subscription = await registration?.pushManager.getSubscription();
  return { supported: true, subscribed: Boolean(subscription) };
}

export async function enablePushNotifications() {
  if (!isPushSupported()) {
    throw new Error("Браузер не поддерживает push-уведомления.");
  }

  const permission = await Notification.requestPermission();
  if (permission !== "granted") {
    throw new Error("Уведомления не разрешены.");
  }

  const registration = await navigator.serviceWorker.register("/sw.js");
  await navigator.serviceWorker.ready;

  const { publicKey } = await api.push.vapidPublicKey();
  if (!publicKey) {
    throw new Error("На сервере не настроены VAPID-ключи — push отключён.");
  }

  // Если подписка уже есть — переиспользуем её, а не пытаемся подписаться заново
  // (повторный subscribe() падает, если подписка с этим ключом уже существует).
  let subscription = await registration.pushManager.getSubscription();
  if (!subscription) {
    try {
      subscription = await registration.pushManager.subscribe({
        userVisibleOnly: true,
        applicationServerKey: urlBase64ToUint8Array(publicKey),
      });
    } catch {
      // Отказ приходит от самого браузера на шаге регистрации в push-сервисе
      // Google (FCM), а не от нашего бэкенда. Частая причина — нет доступа к
      // серверам Google (нужен VPN) или push отключён в настройках браузера.
      throw new Error(
        "Браузер не смог зарегистрироваться в push-сервисе. Обычно причина внешняя: " +
          "нет доступа к серверам Google (попробуйте VPN) или push отключён в браузере.",
      );
    }
  }

  const raw = subscription.toJSON();
  await api.push.subscribe({
    endpoint: raw.endpoint,
    keys: { p256dh: raw.keys.p256dh, auth: raw.keys.auth },
  });

  return subscription;
}

export async function disablePushNotifications() {
  const registration = await navigator.serviceWorker.getRegistration();
  const subscription = await registration?.pushManager.getSubscription();
  if (!subscription) return;

  await api.push.unsubscribe(subscription.endpoint);
  await subscription.unsubscribe();
}
