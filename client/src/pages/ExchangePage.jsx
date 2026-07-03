import { useEffect, useState } from "react";
import { api } from "../api/client.js";
import { useAuth } from "../context/AuthContext.jsx";

function ChatPanel({ offer, otherUserId, otherUserDisplayName, myId, onClose, onExchangeConfirmed }) {
  const [messages, setMessages] = useState([]);
  const [text, setText] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);
  const [confirming, setConfirming] = useState(false);

  const isOwner = String(offer.ownerId) === String(myId);

  // silent — для фонового поллинга: не показываем ошибку, если один опрос не удался
  function load({ silent = false } = {}) {
    api.exchange
      .getMessages(offer.id, otherUserId)
      .then((data) => {
        setMessages(data);
        if (!silent) setError("");
      })
      .catch(() => {
        if (!silent) setError("Не удалось загрузить переписку");
      })
      .finally(() => setLoading(false));
  }

  useEffect(() => {
    load();
    const timer = setInterval(() => load({ silent: true }), 4000);
    return () => clearInterval(timer);
  }, [offer.id, otherUserId]);

  async function send() {
    if (!text.trim()) return;
    setError("");

    try {
      await api.exchange.sendMessage(offer.id, { receiverId: otherUserId, text: text.trim() });
      setText("");
      load();
    } catch {
      setError("Не удалось отправить сообщение");
    }
  }

  async function confirmExchange() {
    setConfirming(true);
    setError("");

    try {
      await api.exchange.confirm(offer.id, { otherUserId });
      onExchangeConfirmed();
    } catch {
      setError("Не удалось подтвердить обмен");
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
  const [error, setError] = useState("");
  const [form, setForm] = useState({ title: "", description: "", wantedPlantDescription: "", userPlantId: "" });
  const [activeChat, setActiveChat] = useState(null);

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
  }, []);

  function updateField(field, value) {
    setForm((current) => ({ ...current, [field]: value }));
  }

  async function createOffer() {
    if (!form.title.trim() || !form.description.trim() || !form.wantedPlantDescription.trim()) {
      setError("Заполните название, описание и что хотите взамен.");
      return;
    }

    setError("");

    try {
      await api.exchange.createOffer({
        title: form.title.trim(),
        description: form.description.trim(),
        wantedPlantDescription: form.wantedPlantDescription.trim(),
        userPlantId: form.userPlantId || null,
      });
      setForm({ title: "", description: "", wantedPlantDescription: "", userPlantId: "" });
      loadOffers();
    } catch {
      setError("Не удалось создать предложение");
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

      <div className="form-panel narrow">
        <input
          className="input"
          value={form.title}
          onChange={(event) => updateField("title", event.target.value)}
          placeholder="Название предложения"
        />
        <textarea
          className="input textarea"
          value={form.description}
          onChange={(event) => updateField("description", event.target.value)}
          placeholder="Описание растения"
        />
        <input
          className="input"
          value={form.wantedPlantDescription}
          onChange={(event) => updateField("wantedPlantDescription", event.target.value)}
          placeholder="Что хотите получить взамен"
        />
        <select
          className="input"
          value={form.userPlantId}
          onChange={(event) => updateField("userPlantId", event.target.value)}
        >
          <option value="">Не привязывать к растению из моего списка</option>
          {myPlants.map((plant) => (
            <option key={plant.id} value={plant.id}>
              {plant.plantName}
            </option>
          ))}
        </select>
        <button className="button" onClick={createOffer}>
          Разместить предложение
        </button>
      </div>

      <h2>Активные предложения</h2>
      <div className="list">
        {offers.map((offer) => {
          const isMine = String(offer.ownerId) === String(myId);

          return (
            <div className="list-item" key={offer.id}>
              <div>
                <h2>{offer.title}</h2>
                {offer.plantName && <p className="latin-name">{offer.plantName}</p>}
                <p className="muted">{offer.description}</p>
                <p className="muted">Хочет взамен: {offer.wantedPlantDescription}</p>
              </div>

              {isMine ? (
                <button className="button button-danger" onClick={() => closeOffer(offer.id)}>
                  Закрыть
                </button>
              ) : (
                <button className="button" onClick={() => openChatWithOwner(offer)}>
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
