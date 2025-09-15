import { createTheme } from '@mui/material/styles';

const getPalette = (mode) => ({
  mode,
  primary: {
    main: '#2E7D32',
    light: '#66BB6A',
    dark: '#1B5E20',
    contrastText: '#ffffff',
  },
  secondary: {
    main: '#FF6F00',
    light: '#FFB74D',
    dark: '#E65100',
    contrastText: '#ffffff',
  },
  background: mode === 'dark'
    ? { default: '#121212', paper: '#1E1E1E' }
    : { default: '#F8F9FA', paper: '#FFFFFF' },
  success: {
    main: '#4CAF50',
    light: '#81C784',
    dark: '#388E3C',
  },
  error: {
    main: '#F44336',
    light: '#EF5350',
    dark: '#D32F2F',
  },
  warning: {
    main: '#FF9800',
    light: '#FFB74D',
    dark: '#F57C00',
  },
  info: {
    main: '#2196F3',
    light: '#64B5F6',
    dark: '#1976D2',
  },
  text: mode === 'dark'
    ? { primary: '#E0E0E0', secondary: '#B0BEC5' }
    : { primary: '#212121', secondary: '#757575' },
  grey: {
    50: '#FAFAFA',
    100: '#F5F5F5',
    200: '#EEEEEE',
    300: '#E0E0E0',
    400: '#BDBDBD',
    500: '#9E9E9E',
    600: '#757575',
    700: '#616161',
    800: '#424242',
    900: '#212121',
  },
  divider: mode === 'dark' ? 'rgba(255,255,255,0.12)' : '#E0E0E0',
  action: {
    hover: mode === 'dark' ? 'rgba(255,255,255,0.08)' : 'rgba(0,0,0,0.04)',
    selected: mode === 'dark' ? 'rgba(46,125,50,0.3)' : 'rgba(46,125,50,0.16)',
  },
});

const createTypography = (fontScale) => {
  const formatFontSize = (size) => `${parseFloat((size * fontScale).toFixed(3))}rem`;

  return {
    fontFamily: '"Inter", "Roboto", "Helvetica", "Arial", sans-serif',
    h1: { fontSize: formatFontSize(2.5), fontWeight: 700, lineHeight: 1.2 },
    h2: { fontSize: formatFontSize(2), fontWeight: 600, lineHeight: 1.3 },
    h3: { fontSize: formatFontSize(1.75), fontWeight: 600, lineHeight: 1.3 },
    h4: { fontSize: formatFontSize(1.5), fontWeight: 600, lineHeight: 1.4 },
    h5: { fontSize: formatFontSize(1.25), fontWeight: 500, lineHeight: 1.4 },
    h6: { fontSize: formatFontSize(1.125), fontWeight: 500, lineHeight: 1.4 },
    subtitle1: { fontSize: formatFontSize(1.0625), lineHeight: 1.5 },
    subtitle2: { fontSize: formatFontSize(0.9375), lineHeight: 1.5 },
    body1: { fontSize: formatFontSize(1), lineHeight: 1.6 },
    body2: { fontSize: formatFontSize(0.875), lineHeight: 1.5 },
    button: { fontSize: formatFontSize(0.875), fontWeight: 500, textTransform: 'none' },
    caption: { fontSize: formatFontSize(0.75), lineHeight: 1.4 },
    overline: { fontSize: formatFontSize(0.75), letterSpacing: '0.1em', fontWeight: 500 },
  };
};

const getComponentOverrides = (mode) => ({
  MuiCssBaseline: {
    styleOverrides: {
      body: {
        backgroundColor: mode === 'dark' ? '#121212' : '#F8F9FA',
        color: mode === 'dark' ? '#E0E0E0' : '#212121',
        colorScheme: mode,
      },
      '*::-webkit-scrollbar': {
        width: 8,
        height: 8,
      },
      '*::-webkit-scrollbar-track': {
        background: mode === 'dark' ? '#2A2A2A' : '#f1f1f1',
        borderRadius: 4,
      },
      '*::-webkit-scrollbar-thumb': {
        background: mode === 'dark' ? '#555555' : '#c1c1c1',
        borderRadius: 4,
      },
      '*::-webkit-scrollbar-thumb:hover': {
        background: mode === 'dark' ? '#666666' : '#a8a8a8',
      },
      '::selection': {
        backgroundColor: mode === 'dark' ? '#388E3C' : '#66BB6A',
        color: '#ffffff',
      },
      '::-moz-selection': {
        backgroundColor: mode === 'dark' ? '#388E3C' : '#66BB6A',
        color: '#ffffff',
      },
      '*:focus-visible': {
        outline: `2px solid ${mode === 'dark' ? '#90CAF9' : '#2E7D32'}`,
        outlineOffset: 2,
      },
      '*': {
        transition: 'color 0.2s ease-in-out, background-color 0.2s ease-in-out, border-color 0.2s ease-in-out',
      },
    },
  },

    MuiButton: {
    styleOverrides: {
      root: {
        borderRadius: 8,
        padding: '8px 24px',
        fontSize: '0.875rem',
        fontWeight: 500,
        boxShadow: 'none',
        '&:hover': {
          boxShadow: '0px 4px 8px rgba(0,0,0,0.1)',
        },
              },
      contained: {
        '&:hover': {
          boxShadow: '0px 4px 12px rgba(0,0,0,0.15)',
        },
      },
    },
      },
  MuiPaper: {
    styleOverrides: {
      root: {
        borderRadius: 12,
        boxShadow: '0px 2px 8px rgba(0,0,0,0.05)',
      },
      elevation1: {
        boxShadow: '0px 2px 8px rgba(0,0,0,0.05)',
      },
      elevation2: {
        boxShadow: '0px 4px 12px rgba(0,0,0,0.1)',
      },
      elevation3: {
        boxShadow: '0px 8px 24px rgba(0,0,0,0.15)',
      },
    },
  },
  MuiCard: {
    styleOverrides: {
      root: {
        borderRadius: 12,
        boxShadow: '0px 2px 8px rgba(0,0,0,0.05)',
        transition: 'all 0.3s ease-in-out',
        '&:hover': {
          boxShadow: '0px 8px 24px rgba(0,0,0,0.15)',
          transform: 'translateY(-2px)',
        },
      },
    },
      },
  MuiTextField: {
    styleOverrides: {
      root: {
        '& .MuiOutlinedInput-root': {
          borderRadius: 8,
        },
      },
    },
      },
  MuiAppBar: {
    styleOverrides: {
      root: {
        backgroundColor: mode === 'dark' ? '#1E1E1E' : '#ffffff',
        color: mode === 'dark' ? '#E0E0E0' : '#212121',
        boxShadow: '0px 2px 8px rgba(0,0,0,0.05)',
        borderBottom: `1px solid ${mode === 'dark' ? 'rgba(255,255,255,0.12)' : '#E0E0E0'}`,
      },
    },
      },
  MuiTableContainer: {
    styleOverrides: {
      root: {
        borderRadius: 12,
        boxShadow: '0px 2px 8px rgba(0,0,0,0.05)',
      },
    },
      },
  MuiTableHead: {
    styleOverrides: {
      root: {
        backgroundColor: mode === 'dark' ? 'rgba(255,255,255,0.04)' : '#F8F9FA',
        '& .MuiTableCell-head': {
          fontWeight: 600,
          fontSize: '0.875rem',
          textTransform: 'uppercase',
          letterSpacing: '0.5px',
          color: mode === 'dark' ? '#E0E0E0' : '#616161',
        },
      },
    },
      },
  MuiDialog: {
    styleOverrides: {
      paper: {
        borderRadius: 16,
        padding: '8px',
      },
    },
      },
  MuiChip: {
    styleOverrides: {
      root: {
        borderRadius: 8,
        fontWeight: 500,
      },
    },
  },
});
const shadows = [
  'none',
  '0px 2px 4px rgba(0,0,0,0.05)',
  '0px 4px 8px rgba(0,0,0,0.1)',
  '0px 8px 16px rgba(0,0,0,0.1)',
  '0px 12px 24px rgba(0,0,0,0.15)',
  '0px 16px 32px rgba(0,0,0,0.15)',
  '0px 20px 40px rgba(0,0,0,0.2)',
  '0px 24px 48px rgba(0,0,0,0.2)',
  '0px 32px 64px rgba(0,0,0,0.25)',
  '0px 40px 80px rgba(0,0,0,0.25)',
  '0px 48px 96px rgba(0,0,0,0.3)',
  '0px 56px 112px rgba(0,0,0,0.3)',
  '0px 64px 128px rgba(0,0,0,0.35)',
  '0px 72px 144px rgba(0,0,0,0.35)',
  '0px 80px 160px rgba(0,0,0,0.4)',
  '0px 88px 176px rgba(0,0,0,0.4)',
  '0px 96px 192px rgba(0,0,0,0.45)',
  '0px 104px 208px rgba(0,0,0,0.45)',
  '0px 112px 224px rgba(0,0,0,0.5)',
  '0px 120px 240px rgba(0,0,0,0.5)',
  '0px 128px 256px rgba(0,0,0,0.55)',
  '0px 136px 272px rgba(0,0,0,0.55)',
  '0px 144px 288px rgba(0,0,0,0.6)',
  '0px 152px 304px rgba(0,0,0,0.6)',
  '0px 160px 320px rgba(0,0,0,0.65)',
];

const createAppTheme = (mode = 'light', fontScale = 1) => createTheme({
  palette: getPalette(mode),
  typography: createTypography(fontScale),
  shape: { borderRadius: 12 },
  shadows,
  components: getComponentOverrides(mode),
});

export default createAppTheme;