export const formatCurrency = (amount: number): string => {
    return new Intl.NumberFormat('uk-UA', {
        style: 'currency',
        currency: 'UAH',
    }).format(amount);
};

export const formatPrice = (price: number): string => {
    return `${price.toLocaleString('uk-UA')} ₴`;
};