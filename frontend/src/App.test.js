import { render, screen } from '@testing-library/react';
import React from 'react';
import LoginPage from './pages/LoginPage';
import { AuthContext } from './context/AuthContext';

jest.mock('./services/apiService', () => ({
  login: jest.fn(),
}));

jest.mock(
  'react-router-dom',
  () => ({
    useNavigate: () => jest.fn(),
  }),
  { virtual: true }
);

const authValue = {
  token: null,
  user: null,
  login: jest.fn(),
  logout: jest.fn(),
};

test('renders the current GranjaTech login screen', () => {
  render(
    <AuthContext.Provider value={authValue}>
      <LoginPage />
    </AuthContext.Provider>
  );

  expect(screen.getByRole('heading', { name: /GranjaTech/i })).toBeInTheDocument();
  expect(screen.getByText(/Sistema de monitoramento e gestão de granjas/i)).toBeInTheDocument();
});
