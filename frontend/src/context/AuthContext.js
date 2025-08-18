import React, { createContext, useState, useEffect } from 'react';
import { jwtDecode } from 'jwt-decode'; // Importe a nova biblioteca
import apiService from '../services/apiService';

export const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
    const [token, setToken] = useState(localStorage.getItem('token'));
    const [user, setUser] = useState(null); // Novo estado para os dados do usuário

    useEffect(() => {
        const tokenFromStorage = localStorage.getItem('token');
        if (tokenFromStorage) {
            try {
                const decodedUser = jwtDecode(tokenFromStorage);
                setUser(decodedUser); // Decodifica o token e salva os dados do usuário
                setToken(tokenFromStorage);
            } catch (error) {
                // Token inválido, limpa o estado
                logout();
            }
        }
    }, []);

    const login = async (email, senha) => {
        const response = await apiService.login({ email, senha });
        const newToken = response.data.token;
        const decodedUser = jwtDecode(newToken);
        
        localStorage.setItem('token', newToken);
        setToken(newToken);
        setUser(decodedUser);
    };

    const logout = () => {
        localStorage.removeItem('token');
        setToken(null);
        setUser(null);
    };

    // Agora o provider também disponibiliza os dados do usuário
    return (
        <AuthContext.Provider value={{ token, user, login, logout }}>
            {children}
        </AuthContext.Provider>
    );
};