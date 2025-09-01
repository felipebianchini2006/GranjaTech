import { render, screen } from '@testing-library/react';
import React from 'react';

// Mock react-router-dom to avoid import errors during testing
jest.mock(
  'react-router-dom',
  () => ({
    BrowserRouter: ({ children }) => <div>{children}</div>,
    Routes: ({ children }) => <div>{children}</div>,
    Route: ({ element }) => <div>{element}</div>,
    Navigate: () => null,
  }),
  { virtual: true }
);

const App = require('./App').default;

test('renders learn react link', () => {
  render(<App />);
  const linkElement = screen.getByText(/learn react/i);
  expect(linkElement).toBeInTheDocument();
});
