#!/usr/bin/env bash
# Быстрая проверка, что ключ и Pl@ntNet API реально работают — БЕЗ нашего приложения.
#
# Использование:
#   bash test-plantnet.sh <API_KEY> [путь_к_фото.jpg]
#
# Без второго аргумента скачает пример фото монстеры.
# Успех = HTTP 200 и в JSON виден "results" с "scientificNameWithoutAuthor".

KEY="$1"
IMG="${2:-/tmp/plantnet-sample.jpg}"

if [ -z "$KEY" ]; then
  echo "Usage: bash test-plantnet.sh <API_KEY> [image.jpg]"
  exit 1
fi

if [ ! -f "$IMG" ]; then
  echo "Скачиваю пример фото монстеры -> $IMG"
  curl -sL -o "$IMG" "https://images.unsplash.com/photo-1614594975525-e45190c55d0b?w=800&q=80"
fi
echo "Фото: $(wc -c < "$IMG") байт"

echo "Отправляю в Pl@ntNet (project=all, organs=auto)..."
echo "-----------------------------------------------------"
# -sS: тихо, но ошибки показываем; -w: печатаем HTTP-код в конце
curl -sS -w "\n-----------------------------------------------------\n[HTTP %{http_code}]\n" \
  -X POST "https://my-api.plantnet.org/v2/identify/all?api-key=${KEY}" \
  -F "images=@${IMG}" \
  -F "organs=auto"
echo "Если видишь results[].species.scientificNameWithoutAuthor и [HTTP 200] — API работает."
