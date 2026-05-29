export const SENSOR_TYPES = [
  { name: 'Temperatura', unit: '°C', defaultValue: '24°C' },
  { name: 'Umidade', unit: '%', defaultValue: '65%' },
  { name: 'Luminosidade', unit: 'lux', defaultValue: '50 lux' },
];

export const SENSOR_TYPE_NAMES = SENSOR_TYPES.map((sensor) => sensor.name);

export const getSensorType = (type) =>
  SENSOR_TYPES.find((sensor) => sensor.name === type);

export const getSensorUnit = (type) => getSensorType(type)?.unit || '';
