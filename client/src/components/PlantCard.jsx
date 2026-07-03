function formatDays(days) {
  if (!days) return null;
  return `раз в ${days} дн.`;
}

function formatMonths(months) {
  if (!months) return null;
  return `раз в ${months} мес.`;
}

export default function PlantCard({ plant, actions }) {
  const watering = formatDays(plant.wateringFrequencyDays);
  const repotting = formatMonths(plant.repottingFrequencyMonths);

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
            {watering && (
              <p>
                <strong>Полив:</strong> {watering}
              </p>
            )}
            {plant.light && (
              <p>
                <strong>Освещение:</strong> {plant.light}
              </p>
            )}
            {repotting && (
              <p>
                <strong>Пересадка:</strong> {repotting}
              </p>
            )}
            {plant.toxicity && (
              <p>
                <strong>Ядовитость:</strong> {plant.toxicity}
              </p>
            )}
            {plant.careFeatures && (
              <p>
                <strong>Особенности:</strong> {plant.careFeatures}
              </p>
            )}
          </div>

          {actions && <div className="plant-card-actions">{actions}</div>}
        </div>
      </div>
    </article>
  );
}
