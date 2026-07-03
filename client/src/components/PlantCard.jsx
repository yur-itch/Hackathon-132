export default function PlantCard({ plant, actions }) {
  return (
    <article className="plant-card" tabIndex={0}>
      <div className="plant-card-summary">
        <h2>{plant.name}</h2>
        {plant.difficulty && <span className="plant-card-badge">{plant.difficulty}</span>}
      </div>

      <div className="plant-card-details">
        <div className="plant-card-details-inner">
          {plant.description && <p className="muted">{plant.description}</p>}

          <div className="plant-facts">
            {plant.watering && (
              <p>
                <strong>Полив:</strong> {plant.watering}
              </p>
            )}
            {plant.light && (
              <p>
                <strong>Освещение:</strong> {plant.light}
              </p>
            )}
            {plant.repotting && (
              <p>
                <strong>Пересадка:</strong> {plant.repotting}
              </p>
            )}
            {plant.toxicity && (
              <p>
                <strong>Ядовитость:</strong> {plant.toxicity}
              </p>
            )}
          </div>

          {actions && <div className="plant-card-actions">{actions}</div>}
        </div>
      </div>
    </article>
  );
}
