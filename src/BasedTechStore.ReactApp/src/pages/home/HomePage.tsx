import { HeroSlider } from '../../components/home/HeroSlider';
import { RecentProducts } from '../../components/home/RecentProducts';

export const HomePage = () => {
    return (
        <div>
            {/*Cyrillic fix*/}
            <h1>Вітаю в Based Tech Store</h1>
            <HeroSlider />
            <RecentProducts />
        </div>
    );
}