import React from 'react';
import { Box, Container, Fade, Breadcrumbs, Typography, Link } from '@mui/material';
import { Link as RouterLink, useLocation } from 'react-router-dom';
import { NavigateNext as NavigateNextIcon } from '@mui/icons-material';

const routeLabels = {
  '/': 'Dashboard',
  '/granjas': 'Granjas',
  '/lotes': 'Lotes',
  '/estoque': 'Estoque',
  '/sensores': 'Sensores',
  '/financeiro': 'Financeiro',
  '/relatorios': 'Relatórios',
  '/usuarios': 'Usuários',
  '/auditoria': 'Auditoria',
  '/perfil': 'Perfil',
};

function PageContainer({ 
  children, 
  title, 
  subtitle,
  maxWidth = 'xl',
  showBreadcrumbs = true,
  action
}) {
  const location = useLocation();
  
  const getBreadcrumbs = () => {
    const pathSegments = location.pathname.split('/').filter(Boolean);
    const breadcrumbs = [
      { label: 'Dashboard', path: '/' }
    ];
    
    if (location.pathname !== '/') {
      let currentPath = '';
      pathSegments.forEach(segment => {
        currentPath += `/${segment}`;
        const label = routeLabels[currentPath] || segment;
        breadcrumbs.push({ label, path: currentPath });
      });
    }
    
    return breadcrumbs;
  };

  return (
    <Box
      component="main"
      sx={{
        flexGrow: 1,
        width: { md: `calc(100% - 280px)` },
        ml: { md: '280px' },
        mt: { xs: 7, md: 8 }, // Account for AppBar height
        minHeight: 'calc(100vh - 64px)',
        backgroundColor: 'background.default',
      }}
    >
      <Container maxWidth={maxWidth} sx={{ py: 3 }}>
        <Fade in timeout={300}>
          <Box>
            {showBreadcrumbs && location.pathname !== '/' && (
              <Breadcrumbs
                separator={<NavigateNextIcon fontSize="small" />}
                sx={{ mb: 2 }}
              >
                {getBreadcrumbs().map((crumb, index) => {
                  const isLast = index === getBreadcrumbs().length - 1;
                  
                  return isLast ? (
                    <Typography key={crumb.path} color="text.primary" fontWeight={500}>
                      {crumb.label}
                    </Typography>
                  ) : (
                    <Link
                      key={crumb.path}
                      component={RouterLink}
                      to={crumb.path}
                      color="inherit"
                      underline="hover"
                      sx={{ fontWeight: 500 }}
                    >
                      {crumb.label}
                    </Link>
                  );
                })}
              </Breadcrumbs>
            )}
            
            {(title || action) && (
              <Box
                sx={{
                  display: 'flex',
                  flexDirection: { xs: 'column', sm: 'row' },
                  alignItems: { xs: 'flex-start', sm: 'center' },
                  justifyContent: 'space-between',
                  gap: 2,
                  mb: 3,
                }}
              >
                <Box>
                  {title && (
                    <Typography
                      variant="h4"
                      component="h1"
                      sx={{
                        fontWeight: 700,
                        color: 'text.primary',
                        mb: subtitle ? 0.5 : 0,
                      }}
                    >
                      {title}
                    </Typography>
                  )}
                  {subtitle && (
                    <Typography
                      variant="body1"
                      color="text.secondary"
                      sx={{ fontWeight: 400 }}
                    >
                      {subtitle}
                    </Typography>
                  )}
                </Box>
                {action && (
                  <Box sx={{ flexShrink: 0 }}>
                    {action}
                  </Box>
                )}
              </Box>
            )}
            
            {children}
          </Box>
        </Fade>
      </Container>
    </Box>
  );
}

export default PageContainer;
