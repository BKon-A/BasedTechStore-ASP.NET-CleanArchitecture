import { createBrowserRouter } from 'react-router-dom';
import { MainLayout } from './components/layout/MainLayout';
import { ROUTES } from './constants/routes';
import { CartPage } from './pages/carts/CartPage';
import { ComparePage } from './pages/compare/ComparePage';
import { ErrorPage } from './pages/errors/ErrorPage';
import { HomePage } from './pages/home/HomePage';
import { ProductsPage } from './pages/products/ProductsPage';
import { WishlistPage } from './pages/wishlist/WishlistPage';
import { AdminDashboard } from './pages/admin-panel/AdminDashboard';
import { ProfilePage } from './pages/profile/ProfilePage';

export const router = createBrowserRouter([
    {
        path: ROUTES.HOME,
        element: <MainLayout />,
        errorElement: <ErrorPage />,
        children: [
            {
                index: true,
                element: <HomePage />,
            },
            {
                path: ROUTES.PROFILE,
                element: <ProfilePage />,
            },
            {
                path: ROUTES.ADMIN_DASHBOARD.DASHBOARD,
                element: <AdminDashboard />,
            },
            {
                path: ROUTES.PRODUCTS,
                element: <ProductsPage />,
            },
            {
                path: ROUTES.CART,
                element: <CartPage />, 
            },
            {
                path: ROUTES.WISHLIST,
                element: <WishlistPage />,
            },
            {
                path: ROUTES.COMPARE,
                element: <ComparePage />,
            }
        ]
    }
]);