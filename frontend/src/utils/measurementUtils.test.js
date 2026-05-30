import {
  formatCurrency,
  formatMeasurement,
  formatPercentage,
  formatSensorMeasurement,
  normalizeUnit,
} from './measurementUtils';

describe('measurementUtils', () => {
  test('normalizes common unit aliases without changing numeric values', () => {
    expect(normalizeUnit('Kg')).toBe('kg');
    expect(normalizeUnit('quilos')).toBe('kg');
    expect(normalizeUnit('gramas')).toBe('g');
    expect(normalizeUnit('litros')).toBe('L');
    expect(normalizeUnit('ml')).toBe('mL');
    expect(normalizeUnit('unidades')).toBe('un');
    expect(normalizeUnit('dose')).toBe('doses');
    expect(normalizeUnit('saco')).toBe('sacos');
  });

  test('formats measurements with pt-BR numbers and clean spacing', () => {
    expect(formatMeasurement(1500.5, 'kg', { maximumFractionDigits: 1 })).toBe('1.500,5 kg');
    expect(formatMeasurement(65, '%')).toBe('65%');
    expect(formatMeasurement(28, 'temperatura')).toBe('28 °C');
    expect(formatMeasurement(300, 'luminosidade')).toBe('300 lux');
    expect(formatMeasurement(12, 'aves')).toBe('12 aves');
  });

  test('formats currency and percentages in Brazilian display style', () => {
    expect(formatCurrency(1250.5)).toBe('R$ 1.250,50');
    expect(formatPercentage(12.345, { maximumFractionDigits: 1 })).toBe('12,3%');
  });

  test('formats sensor values by type', () => {
    expect(formatSensorMeasurement(24, 'Temperatura')).toBe('24 °C');
    expect(formatSensorMeasurement(65, 'Umidade')).toBe('65%');
    expect(formatSensorMeasurement(50, 'Luminosidade')).toBe('50 lux');
  });
});
