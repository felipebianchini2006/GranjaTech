import { SENSOR_TYPES, SENSOR_TYPE_NAMES, getSensorUnit } from './sensorTypes';

test('exposes only supported sensor types', () => {
  expect(SENSOR_TYPE_NAMES).toEqual(['Temperatura', 'Umidade', 'Luminosidade']);
  expect(SENSOR_TYPE_NAMES).not.toEqual(expect.arrayContaining(['Pressão', 'Pressao', 'pH', 'Outros']));
});

test('maps supported sensor types to their display units', () => {
  expect(getSensorUnit('Temperatura')).toBe('°C');
  expect(getSensorUnit('Umidade')).toBe('%');
  expect(getSensorUnit('Luminosidade')).toBe('lux');
});

test('keeps default sensor cards aligned with supported types', () => {
  expect(SENSOR_TYPES.map((sensor) => sensor.defaultValue)).toEqual(['24°C', '65%', '50 lux']);
});
