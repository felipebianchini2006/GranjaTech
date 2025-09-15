import React, { createContext, useCallback, useEffect, useMemo, useState } from 'react';

export const DEFAULT_COLOR_MODE = 'light';
export const DEFAULT_FONT_SCALE = 1;
export const FONT_SCALE_STEP = 0.1;
export const MIN_FONT_SCALE = 0.85;
export const MAX_FONT_SCALE = 1.3;

const STORAGE_KEY = 'granjatech-accessibility-preferences';

export const AccessibilityContext = createContext({
  mode: DEFAULT_COLOR_MODE,
  fontScale: DEFAULT_FONT_SCALE,
  toggleColorMode: () => {},
  increaseFontScale: () => {},
  decreaseFontScale: () => {},
  resetSettings: () => {},
  canIncreaseFont: true,
  canDecreaseFont: true,
});

const clampFontScale = (value) => {
  if (Number.isNaN(value)) {
    return DEFAULT_FONT_SCALE;
  }
  return Math.min(MAX_FONT_SCALE, Math.max(MIN_FONT_SCALE, value));
};

const readStoredPreferences = () => {
  if (typeof window === 'undefined') {
    return null;
  }

  try {
    const storedValue = window.localStorage.getItem(STORAGE_KEY);
    if (!storedValue) {
      return null;
    }

    const parsed = JSON.parse(storedValue);
    if (!parsed || typeof parsed !== 'object') {
      return null;
    }

    const mode = parsed.mode === 'dark' ? 'dark' : DEFAULT_COLOR_MODE;
    const fontScale = clampFontScale(parseFloat(parsed.fontScale));

    return { mode, fontScale };
  } catch (error) {
    console.warn('Não foi possível carregar preferências de acessibilidade:', error);
    return null;
  }
};

export const AccessibilityProvider = ({ children }) => {
  const [storedPreferences] = useState(() => readStoredPreferences());
  const [mode, setMode] = useState(storedPreferences?.mode || DEFAULT_COLOR_MODE);
  const [fontScale, setFontScale] = useState(storedPreferences?.fontScale || DEFAULT_FONT_SCALE);

  useEffect(() => {
    if (typeof window === 'undefined') {
      return;
    }

    if (mode === DEFAULT_COLOR_MODE && fontScale === DEFAULT_FONT_SCALE) {
      window.localStorage.removeItem(STORAGE_KEY);
      return;
    }

    window.localStorage.setItem(
      STORAGE_KEY,
      JSON.stringify({ mode, fontScale })
    );
  }, [mode, fontScale]);

  const toggleColorMode = useCallback(() => {
    setMode((prevMode) => (prevMode === 'light' ? 'dark' : 'light'));
  }, []);

  const increaseFontScale = useCallback(() => {
    setFontScale((prev) => clampFontScale(prev + FONT_SCALE_STEP));
  }, []);

  const decreaseFontScale = useCallback(() => {
    setFontScale((prev) => clampFontScale(prev - FONT_SCALE_STEP));
  }, []);

  const resetSettings = useCallback(() => {
    setMode(DEFAULT_COLOR_MODE);
    setFontScale(DEFAULT_FONT_SCALE);
    if (typeof window !== 'undefined') {
      window.localStorage.removeItem(STORAGE_KEY);
    }
  }, []);

  const contextValue = useMemo(() => ({
    mode,
    fontScale,
    toggleColorMode,
    increaseFontScale,
    decreaseFontScale,
    resetSettings,
    canIncreaseFont: fontScale < MAX_FONT_SCALE - 1e-3,
    canDecreaseFont: fontScale > MIN_FONT_SCALE + 1e-3,
  }), [mode, fontScale, toggleColorMode, increaseFontScale, decreaseFontScale, resetSettings]);

  return (
    <AccessibilityContext.Provider value={contextValue}>
      {children}
    </AccessibilityContext.Provider>
  );
};