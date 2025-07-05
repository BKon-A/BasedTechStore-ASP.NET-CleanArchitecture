import { Container } from "react-bootstrap";
import { Outlet } from "react-router-dom";
import { Header } from "../header/Header";
import { Footer } from "../footer/Footer";

export const MainLayout = () => {
    return (
        <div className="d-flex flex-column min-vh-100">
            <Header />
            <Container as="main" className="flex-grow-1 py-4">
                <Outlet />
            </Container>
            <Footer />
        </div>
    );
}