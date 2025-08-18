import React, { useContext } from 'react';
import { Navigate } from 'react-router-dom';
import { AuthContext } from '../context/AuthContext';

const ProtectedRoute = ({ children }) => {
    const { token } = useContext(AuthContext);

    if (!token) {
        // Se não há token, redireciona para a página de login
        return <Navigate to="/login" />;
    }

    // Se há token, renderiza a página solicitada
    return children;
};

export default ProtectedRoute;