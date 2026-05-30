import { getSensorUnit } from '../constants/sensorTypes';

const UNIT_ALIASES = {
  kg: 'kg',
  kgs: 'kg',
  quilo: 'kg',
  quilos: 'kg',
  quilograma: 'kg',
  quilogramas: 'kg',
  g: 'g',
  grama: 'g',
  gramas: 'g',
  l: 'L',
  lt: 'L',
  litro: 'L',
  litros: 'L',
  ml: 'mL',
  mililitro: 'mL',
  mililitros: 'mL',
  un: 'un',
  unidade: 'un',
  unidades: 'un',
  dose: 'doses',
  doses: 'doses',
  saco: 'sacos',
  sacos: 'sacos',
  ave: 'aves',
  aves: 'aves',
  ovo: 'ovos',
  ovos: 'ovos',
  dia: 'dias',
  dias: 'dias',
  temperatura: '°C',
  umidade: '%',
  percentual: '%',
  porcentagem: '%',
  luminosidade: 'lux',
  lux: 'lux',
  currency: 'R$',
  moeda: 'R$',
  dinheiro: 'R$',
  area: 'm²',
  m2: 'm²',
  'm²': 'm²',
};

const compactUnitSpacing = new Set(['%', 'R$']);

export const formatNumber = (
  value,
  { minimumFractionDigits = 0, maximumFractionDigits = 2 } = {}
) => {
  const number = Number(value);
  if (!Number.isFinite(number)) return '-';

  return number.toLocaleString('pt-BR', {
    minimumFractionDigits,
    maximumFractionDigits,
  });
};

const normalizeKey = (unit) =>
  String(unit || '')
    .trim()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .toLowerCase();

export const normalizeUnit = (unit) => {
  const raw = String(unit || '').trim();
  if (!raw) return '';

  const key = normalizeKey(raw);
  return UNIT_ALIASES[key] || raw;
};

export const formatCurrency = (value) => {
  const number = Number(value);
  if (!Number.isFinite(number)) return '-';

  return number.toLocaleString('pt-BR', {
    style: 'currency',
    currency: 'BRL',
  }).replace(/\u00a0/g, ' ');
};

export const formatPercentage = (
  value,
  { minimumFractionDigits = 0, maximumFractionDigits = 2 } = {}
) => {
  const formatted = formatNumber(value, {
    minimumFractionDigits,
    maximumFractionDigits,
  });

  return formatted === '-' ? '-' : `${formatted}%`;
};

export const formatMeasurement = (
  value,
  unit,
  { minimumFractionDigits = 0, maximumFractionDigits = 2 } = {}
) => {
  const displayUnit = normalizeUnit(unit);
  if (displayUnit === 'R$') return formatCurrency(value);
  if (displayUnit === '%') {
    return formatPercentage(value, { minimumFractionDigits, maximumFractionDigits });
  }

  const formatted = formatNumber(value, {
    minimumFractionDigits,
    maximumFractionDigits,
  });

  if (formatted === '-' || !displayUnit) return formatted;
  return compactUnitSpacing.has(displayUnit)
    ? `${displayUnit}${formatted}`
    : `${formatted} ${displayUnit}`;
};

export const formatSensorMeasurement = (
  value,
  sensorType,
  options = {}
) => formatMeasurement(value, getSensorUnit(sensorType), options);

export const formatCount = (value, unit = 'un', options = {}) =>
  formatMeasurement(value, unit, { maximumFractionDigits: 0, ...options });

export const formatDays = (value, options = {}) =>
  formatMeasurement(value, 'dias', { maximumFractionDigits: 0, ...options });
