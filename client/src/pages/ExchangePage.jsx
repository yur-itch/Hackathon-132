import { useEffect, useState } from "react";
import { HubConnectionBuilder } from "@microsoft/signalr";
import { api } from "../api/client.js";
import { API_URL } from "../api/apiClient.js";
import { useAuth } from "../context/AuthContext.jsx";

function ChatPanel({ offer, otherUserId, otherUserDisplayName, myId, onClose, onExchangeConfirmed }) {
  const [messages, setMessages] = useState([]);
  const [text, setText] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);
  const [confirming, setConfirming] = useState(false);

  const isOwner = String(offer.ownerId) === String(myId);

  function load() {
    api.exchange
      .getMessages(offer.id, otherUserId)
      .then((data) => {
        setMessages(data);
        setError("");
      })
      .catch((err) => {
        // 403 приходит с понятным текстом (нет нужного растения в коллекции)
        setError(err.message || "Не удалось загрузить переписку");
      })
      .finally(() => setLoading(false));
  }

  useEffect(() => {
    load();

    const connection = new HubConnectionBuilder()
      .withUrl(`${API_URL}/hubs/chat`)
      .withAutomaticReconnect()
      .build();

    connection.on("ReceiveMessage", (newMessage) => {
      setMessages((prev) => {
        if (prev.some((m) => m.id === newMessage.id)) {
          return prev;
        }
        return [...prev, newMessage];
      });
    });

    connection
      .start()
      .then(() => {
        connection.invoke("JoinChat", offer.id.toString(), otherUserId.toString());
      })
      .catch((err) => {
        console.error("SignalR Connection Error: ", err);
      });

    return () => {
      connection.stop();
    };
  }, [offer.id, otherUserId]);

  async function send() {
    if (!text.trim()) return;
    setError("");

    try {
      await api.exchange.sendMessage(offer.id, { receiverId: otherUserId, text: text.trim() });
      setText("");
    } catch (err) {
      setError(err.message || "Не удалось отправить сообщение");
    }
  }

  async function confirmExchange() {
    setConfirming(true);
    setError("");

    try {
      await api.exchange.confirm(offer.id, { otherUserId });
      onExchangeConfirmed();
    } catch (err) {
      // 409 с текстом: у собеседника больше нет растения / своё растение пропало
      setError(err.message || "Не удалось подтвердить обмен");
    } finally {
      setConfirming(false);
    }
  }

  return (
    <div className="result-box">
      <div className="page-title">
        <h2>Чат: {offer.title}</h2>
        <p className="muted">Собеседник: {otherUserDisplayName || otherUserId}</p>
      </div>

      {loading && <p className="muted">Загрузка…</p>}
      {error && <p className="error">{error}</p>}

      <div className="list">
        {messages.map((message) => (
          <div className="list-item" key={message.id}>
            <div>
              <p className="muted">{String(message.senderId) === String(myId) ? "Вы" : otherUserDisplayName}</p>
              <p>{message.text}</p>
            </div>
          </div>
        ))}
      </div>

      {messages.length === 0 && !loading && <p className="muted">Сообщений пока нет.</p>}

      <div className="form-row" style={{ marginTop: 16 }}>
        <input
          className="input"
          value={text}
          onChange={(event) => setText(event.target.value)}
          placeholder="Написать сообщение…"
        />
        <button className="button" onClick={send}>
          Отправить
        </button>
      </div>

      <div className="form-row" style={{ marginTop: 12 }}>
        {isOwner && offer.isActive && (
          <button className="button" onClick={confirmExchange} disabled={confirming}>
            {confirming ? "Подождите…" : "Подтвердить обмен с этим человеком"}
          </button>
        )}
        <button className="button button-secondary" onClick={onClose}>
          Закрыть чат
        </button>
      </div>
    </div>
  );
}

export default function ExchangePage() {
  const { user } = useAuth();
  const myId = user?.id;

  const [offers, setOffers] = useState([]);
  const [chats, setChats] = useState([]);
  const [myPlants, setMyPlants] = useState([]);
  const [catalog, setCatalog] = useState([]);
  const [error, setError] = useState("");
  const [form, setForm] = useState({ title: "", description: "", wantedPlantId: "", userPlantId: "" });
  const [activeChat, setActiveChat] = useState(null);

  // Каталожные id растений, которые есть у меня в коллекции — по ним решаем,
  // могу ли я откликнуться на объявление (нужно иметь желаемое растение).
  const myCatalogPlantIds = new Set(myPlants.map((plant) => plant.plantId));

  function loadOffers() {
    api.exchange.listOffers().then(setOffers).catch(() => setError("Не удалось загрузить предложения"));
  }

  function loadChats() {
    api.exchange.listChats().then(setChats).catch(() => setError("Не удалось загрузить переписки"));
  }

  useEffect(() => {
    loadOffers();
    loadChats();
    api.userPlants.listMine().then(setMyPlants).catch(() => {});
    api.plants.list().then(setCatalog).catch(() => {});
  }, []);

  function updateField(field, value) {
    setForm((current) => ({ ...current, [field]: value }));
  }

  async function createOffer() {
    if (!form.title.trim()) {
      setError("Укажите название объявления.");
      return;
    }
    if (!form.userPlantId) {
      setError("Выберите своё растение для обмена.");
      return;
    }
    if (!form.wantedPlantId) {
      setError("Выберите растение, которое хотите получить.");
      return;
    }

    setError("");

    try {
      await api.exchange.createOffer({
        title: form.title.trim(),
        description: form.description.trim(),
        userPlantId: form.userPlantId,
        wantedPlantId: Number(form.wantedPlantId),
      });
      setForm({ title: "", description: "", wantedPlantId: "", userPlantId: "" });
      loadOffers();
    } catch (err) {
      setError(err.message || "Не удалось создать предложение");
    }
  }

  async function closeOffer(id) {
    try {
      await api.exchange.closeOffer(id);
      loadOffers();
    } catch {
      setError("Не удалось закрыть предложение");
    }
  }

  function openChatWithOwner(offer) {
    setActiveChat({ offer, otherUserId: offer.ownerId, otherUserDisplayName: null });
  }

  function openChatFromList(chat) {
    const offer = offers.find((item) => item.id === chat.exchangeOfferId) ?? {
      id: chat.exchangeOfferId,
      title: chat.exchangeOfferTitle,
      ownerId: myId,
      isActive: true,
    };
    setActiveChat({ offer, otherUserId: chat.otherUserId, otherUserDisplayName: chat.otherUserDisplayName });
  }

  return (
    <section>
      <div className="page-title">
        <h1>Обмен растениями</h1>
        <p>Разместите своё растение для обмена или откликнитесь на чужое предложение.</p>
      </div>

      {error && <p className="error">{error}</p>}

      {myPlants.length === 0 ? (
        <p className="muted">
          Чтобы предложить обмен, добавьте хотя бы одно растение в раздел «Мои растения».
        </p>
      ) : (
        <div className="form-panel narrow">
          <input
            className="input"
            value={form.title}
            onChange={(event) => updateField("title", event.target.value)}
            placeholder="Название объявления"
          />
          <textarea
            className="input textarea"
            value={form.description}
            onChange={(event) => updateField("description", event.target.value)}
            placeholder="Описание (состояние растения, детали) — необязательно"
          />

          <label>
            Отдаю (из моей коллекции)
            <select
              className="input"
              value={form.userPlantId}
              onChange={(event) => updateField("userPlantId", event.target.value)}
            >
              <option value="">Выберите своё растение</option>
              {myPlants.map((plant) => (
                <option key={plant.id} value={plant.id}>
                  {plant.plantName}
                </option>
              ))}
            </select>
          </label>

          <label>
            Хочу получить (из справочника)
            <select
              className="input"
              value={form.wantedPlantId}
              onChange={(event) => updateField("wantedPlantId", event.target.value)}
            >
              <option value="">Выберите желаемое растение</option>
              {catalog.map((plant) => (
                <option key={plant.id} value={plant.id}>
                  {plant.name}
                </option>
              ))}
            </select>
          </label>

          <button className="button" onClick={createOffer}>
            Разместить объявление
          </button>
        </div>
      )}

      <h2>Активные предложения</h2>
      <div className="list">
        {offers.map((offer) => {
          const isMine = String(offer.ownerId) === String(myId);
          // Откликнуться можно, только если желаемое растение есть в моей коллекции
          const canRespond = myCatalogPlantIds.has(offer.wantedPlantId);

          return (
            <div className="list-item" key={offer.id}>
              <div>
                <h2>{offer.title}</h2>
                <p className="muted">
                  Отдаёт: <strong>{offer.offeredPlantName ?? "—"}</strong>
                  {" · "}
                  Хочет получить: <strong>{offer.wantedPlantName ?? "—"}</strong>
                </p>
                {offer.description && <p className="muted">{offer.description}</p>}
                {!isMine && !canRespond && (
                  <p className="muted">
                    Чтобы откликнуться, нужно иметь «{offer.wantedPlantName}» в своей коллекции.
                  </p>
                )}
              </div>

              {isMine ? (
                <button className="button button-danger" onClick={() => closeOffer(offer.id)}>
                  Закрыть
                </button>
              ) : (
                <button
                  className="button"
                  onClick={() => openChatWithOwner(offer)}
                  disabled={!canRespond}
                  title={canRespond ? undefined : "Нужно нужное растение в коллекции"}
                >
                  Написать
                </button>
              )}
            </div>
          );
        })}
      </div>

      {offers.length === 0 && <p className="muted">Активных предложений пока нет.</p>}

      <h2>Мои переписки</h2>
      <div className="list">
        {chats.map((chat) => (
          <div className="list-item" key={`${chat.exchangeOfferId}-${chat.otherUserId}`}>
            <div>
              <h2>{chat.exchangeOfferTitle}</h2>
              <p className="muted">{chat.otherUserDisplayName}</p>
              <p className="muted">{chat.lastMessageText}</p>
            </div>
            <button className="button button-secondary" onClick={() => openChatFromList(chat)}>
              Открыть
            </button>
          </div>
        ))}
      </div>

      {chats.length === 0 && <p className="muted">Переписок пока нет.</p>}

      {activeChat && (
        <ChatPanel
          offer={activeChat.offer}
          otherUserId={activeChat.otherUserId}
          otherUserDisplayName={activeChat.otherUserDisplayName}
          myId={myId}
          onClose={() => setActiveChat(null)}
          onExchangeConfirmed={() => {
            setActiveChat(null);
            loadOffers();
            loadChats();
          }}
        />
      )}
    </section>
  );
}
