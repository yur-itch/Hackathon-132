// Контракт API — единый источник правды для фронта.
// Зеркалит модели/DTO бэкенда (server/Models, server/Dtos).
// ВАЖНО: держите синхронно с бэком. Можно генерировать автоматически
// из /openapi/v1.json (см. README, npm run gen:api).

export type Difficulty = "easy" | "medium" | "hard";
export type ReminderType = "Watering" | "Repotting" | "Fertilizing";

export interface Plant {
  id: number;
  name: string;
  latinName?: string | null;
  description?: string | null;
  imageUrl?: string | null;
  light: string;
  wateringFrequencyDays: number;
  repottingFrequencyMonths?: number | null;
  humidity?: string | null;
  temperature?: string | null;
  toxicity?: string | null;
  difficulty: Difficulty;
}

export interface UserPlant {
  id: number;
  ownerId: string;
  plantId?: number | null;
  plant?: Plant | null;
  nickname: string;
  location?: string | null;
  notes?: string | null;
  addedAt: string;
}

export interface Reminder {
  id: number;
  userPlantId: number;
  type: ReminderType;
  intervalDays: number;
  nextDueAt: string;
  lastDoneAt?: string | null;
  enabled: boolean;
}

// --- запросы ---
export interface CreateUserPlantDto {
  plantId?: number | null;
  nickname: string;
  location?: string | null;
  notes?: string | null;
}

export interface CreateReminderDto {
  userPlantId: number;
  type: ReminderType;
  intervalDays: number;
  nextDueAt?: string | null;
}

// --- распознавание по фото ---
export type MatchStatus = "Matched" | "RecognizedButNoCard" | "LowConfidence" | "Failed";

export interface CandidateDto {
  latinName?: string | null;
  commonName?: string | null;
  score: number;
  gbifId?: string | null;
}

export interface RecognitionResult {
  status: MatchStatus;
  recognizedLatinName?: string | null;
  recognizedCommonName?: string | null;
  topScore?: number | null;
  matchedCard?: Plant | null;
  candidates: CandidateDto[];
  message?: string | null;
}
