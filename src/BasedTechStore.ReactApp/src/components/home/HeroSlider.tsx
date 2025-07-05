import { Carousel, Container } from 'react-bootstrap';

export const HeroSlider = () => {
    return (
        <Carousel className="mb-5">
            <Carousel.Item>
                <div className="bg-primary text-white d-flex align-items-center justify-content-center" style={{ height: '400px' }}>
                    <Container className="text-center">
                        <h1 className="display-4 fw-bold">Ласкаво просимо до BasedTechStore</h1>
                        <p className="lead">Найкращі технології за доступними цінами</p>
                    </Container>
                </div>
            </Carousel.Item>
            <Carousel.Item>
                <div className="bg-success text-white d-flex align-items-center justify-content-center" style={{ height: '400px' }}>
                    <Container className="text-center">
                        <h1 className="display-4 fw-bold">Нові надходження</h1>
                        <p className="lead">Перегляньте останні новинки в світі технологій</p>
                    </Container>
                </div>
            </Carousel.Item>
            <Carousel.Item>
                <div className="bg-info text-white d-flex align-items-center justify-content-center" style={{ height: '400px' }}>
                    <Container className="text-center">
                        <h1 className="display-4 fw-bold">Спеціальні пропозиції</h1>
                        <p className="lead">Знижки до 50% на вибрані товари</p>
                    </Container>
                </div>
            </Carousel.Item>
        </Carousel>
    );
}
