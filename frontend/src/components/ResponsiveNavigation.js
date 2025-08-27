import React, { useState, useContext } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { AuthContext } from '../context/AuthContext';
import {
  AppBar,
  Toolbar,
  Typography,
  IconButton,
  Drawer,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  ListItemButton,
  Divider,
  Box,
  Avatar,
  Chip,
  useTheme,
  useMediaQuery,
  Button,
  Menu,
  MenuItem,
} from '@mui/material';
import {
  Menu as MenuIcon,
  Dashboard as DashboardIcon,
  Agriculture as AgricultureIcon,
  Pets as PetsIcon,
  Inventory as InventoryIcon,
  Sensors as SensorsIcon,
  AttachMoney as MoneyIcon,
  Assessment as ReportsIcon,
  People as PeopleIcon,
  Security as SecurityIcon,
  Person as PersonIcon,
  Logout as LogoutIcon,
  Close as CloseIcon,
} from '@mui/icons-material';

const drawerWidth = 280;

const navigationItems = [
  { path: '/', label: 'Dashboard', icon: DashboardIcon, roles: ['Administrador', 'Produtor', 'Financeiro'] },
  { path: '/granjas', label: 'Granjas', icon: AgricultureIcon, roles: ['Administrador', 'Produtor', 'Financeiro'] },
  { path: '/lotes', label: 'Lotes', icon: PetsIcon, roles: ['Administrador', 'Produtor', 'Financeiro'] },
  { path: '/estoque', label: 'Estoque', icon: InventoryIcon, roles: ['Administrador', 'Produtor'] },
  { path: '/avicultura', label: 'Avicultura Pro', icon: SensorsIcon, roles: ['Administrador', 'Produtor'] },
  { path: '/consumo', label: 'Consumo', icon: InventoryIcon, roles: ['Administrador', 'Produtor'] },
  { path: '/pesagem', label: 'Pesagens', icon: PetsIcon, roles: ['Administrador', 'Produtor'] },
  { path: '/sanitario', label: 'Sanitário', icon: SensorsIcon, roles: ['Administrador', 'Produtor'] },
  { path: '/sensores', label: 'Sensores', icon: SensorsIcon, roles: ['Administrador', 'Produtor'] },
  { path: '/financeiro', label: 'Financeiro', icon: MoneyIcon, roles: ['Administrador', 'Financeiro'] },
  { path: '/relatorios', label: 'Relatórios', icon: ReportsIcon, roles: ['Administrador', 'Financeiro', 'Produtor'] },
  { path: '/usuarios', label: 'Usuários', icon: PeopleIcon, roles: ['Administrador'] },
  { path: '/auditoria', label: 'Auditoria', icon: SecurityIcon, roles: ['Administrador'] },
];

function ResponsiveNavigation() {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const location = useLocation();
  const { token, user, logout } = useContext(AuthContext);
  
  const [mobileOpen, setMobileOpen] = useState(false);
  const [anchorEl, setAnchorEl] = useState(null);

  const handleDrawerToggle = () => {
    setMobileOpen(!mobileOpen);
  };

  const handleProfileMenuOpen = (event) => {
    setAnchorEl(event.currentTarget);
  };

  const handleProfileMenuClose = () => {
    setAnchorEl(null);
  };

  const handleLogout = () => {
    logout();
    handleProfileMenuClose();
  };

  const getFilteredNavItems = () => {
    if (!user) return [];
    return navigationItems.filter(item => item.roles.includes(user.role));
  };

  const isActive = (path) => {
    return location.pathname === path;
  };

  const drawer = (
    <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      <Box sx={{ p: 3, display: 'flex', alignItems: 'center', gap: 2 }}>
        <AgricultureIcon sx={{ fontSize: 32, color: 'primary.main' }} />
        <Typography variant="h6" sx={{ fontWeight: 700, color: 'primary.main' }}>
          GranjaTech
        </Typography>
        {isMobile && (
          <IconButton 
            onClick={handleDrawerToggle}
            sx={{ ml: 'auto' }}
          >
            <CloseIcon />
          </IconButton>
        )}
      </Box>
      
      <Divider />
      
      {user && (
        <Box sx={{ p: 2 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
            <Avatar sx={{ bgcolor: 'primary.main', width: 40, height: 40 }}>
              {user.name?.charAt(0) || user.email?.charAt(0)}
            </Avatar>
            <Box sx={{ flex: 1, minWidth: 0 }}>
              <Typography variant="subtitle2" noWrap sx={{ fontWeight: 600 }}>
                {user.name || user.email}
              </Typography>
              <Chip 
                label={user.role} 
                size="small" 
                color="primary" 
                variant="outlined"
                sx={{ fontSize: '0.75rem', height: 20 }}
              />
            </Box>
          </Box>
        </Box>
      )}
      
      <Divider />
      
      <List sx={{ flex: 1, pt: 1 }}>
        {getFilteredNavItems().map((item) => {
          const Icon = item.icon;
          const active = isActive(item.path);
          
          return (
            <ListItem key={item.path} disablePadding sx={{ px: 2, mb: 0.5 }}>
              <ListItemButton
                component={Link}
                to={item.path}
                onClick={isMobile ? handleDrawerToggle : undefined}
                sx={{
                  borderRadius: 2,
                  minHeight: 48,
                  backgroundColor: active ? 'primary.main' : 'transparent',
                  color: active ? 'white' : 'text.primary',
                  '&:hover': {
                    backgroundColor: active ? 'primary.dark' : 'action.hover',
                  },
                  transition: 'all 0.2s ease-in-out',
                }}
              >
                <ListItemIcon
                  sx={{
                    color: active ? 'white' : 'primary.main',
                    minWidth: 40,
                  }}
                >
                  <Icon />
                </ListItemIcon>
                <ListItemText 
                  primary={item.label}
                  primaryTypographyProps={{
                    fontSize: '0.875rem',
                    fontWeight: active ? 600 : 500,
                  }}
                />
              </ListItemButton>
            </ListItem>
          );
        })}
      </List>
      
      <Divider />
      
      <Box sx={{ p: 2 }}>
        <ListItemButton
          onClick={handleLogout}
          sx={{
            borderRadius: 2,
            minHeight: 48,
            color: 'error.main',
            '&:hover': {
              backgroundColor: 'error.light',
              color: 'white',
            },
            transition: 'all 0.2s ease-in-out',
          }}
        >
          <ListItemIcon sx={{ color: 'inherit', minWidth: 40 }}>
            <LogoutIcon />
          </ListItemIcon>
          <ListItemText 
            primary="Sair"
            primaryTypographyProps={{
              fontSize: '0.875rem',
              fontWeight: 500,
            }}
          />
        </ListItemButton>
      </Box>
    </Box>
  );

  if (!token) {
    return null;
  }

  return (
    <Box sx={{ display: 'flex' }}>
      <AppBar
        position="fixed"
        sx={{
          width: { md: `calc(100% - ${drawerWidth}px)` },
          ml: { md: `${drawerWidth}px` },
          backgroundColor: 'background.paper',
          color: 'text.primary',
          boxShadow: '0px 2px 8px rgba(0,0,0,0.05)',
          borderBottom: '1px solid',
          borderColor: 'divider',
        }}
      >
        <Toolbar>
          <IconButton
            color="inherit"
            aria-label="open drawer"
            edge="start"
            onClick={handleDrawerToggle}
            sx={{ mr: 2, display: { md: 'none' } }}
          >
            <MenuIcon />
          </IconButton>
          
          <Typography variant="h6" noWrap component="div" sx={{ flexGrow: 1 }}>
            {navigationItems.find(item => isActive(item.path))?.label || 'GranjaTech'}
          </Typography>
          
          <Button
            onClick={handleProfileMenuOpen}
            startIcon={
              <Avatar sx={{ width: 32, height: 32, bgcolor: 'primary.main' }}>
                {user?.name?.charAt(0) || user?.email?.charAt(0)}
              </Avatar>
            }
            sx={{
              color: 'text.primary',
              textTransform: 'none',
              '&:hover': {
                backgroundColor: 'action.hover',
              },
            }}
          >
            {user?.name || user?.email}
          </Button>
          
          <Menu
            anchorEl={anchorEl}
            open={Boolean(anchorEl)}
            onClose={handleProfileMenuClose}
            transformOrigin={{ horizontal: 'right', vertical: 'top' }}
            anchorOrigin={{ horizontal: 'right', vertical: 'bottom' }}
          >
            <MenuItem 
              component={Link} 
              to="/perfil" 
              onClick={handleProfileMenuClose}
            >
              <ListItemIcon>
                <PersonIcon fontSize="small" />
              </ListItemIcon>
              Perfil
            </MenuItem>
            <Divider />
            <MenuItem onClick={handleLogout}>
              <ListItemIcon>
                <LogoutIcon fontSize="small" />
              </ListItemIcon>
              Sair
            </MenuItem>
          </Menu>
        </Toolbar>
      </AppBar>
      
      <Box
        component="nav"
        sx={{ width: { md: drawerWidth }, flexShrink: { md: 0 } }}
      >
        <Drawer
          variant="temporary"
          open={mobileOpen}
          onClose={handleDrawerToggle}
          ModalProps={{
            keepMounted: true,
          }}
          sx={{
            display: { xs: 'block', md: 'none' },
            '& .MuiDrawer-paper': { 
              boxSizing: 'border-box', 
              width: drawerWidth,
              backgroundColor: 'background.paper',
              borderRight: '1px solid',
              borderColor: 'divider',
            },
          }}
        >
          {drawer}
        </Drawer>
        
        <Drawer
          variant="permanent"
          sx={{
            display: { xs: 'none', md: 'block' },
            '& .MuiDrawer-paper': { 
              boxSizing: 'border-box', 
              width: drawerWidth,
              backgroundColor: 'background.paper',
              borderRight: '1px solid',
              borderColor: 'divider',
            },
          }}
          open
        >
          {drawer}
        </Drawer>
      </Box>
    </Box>
  );
}

export default ResponsiveNavigation;
