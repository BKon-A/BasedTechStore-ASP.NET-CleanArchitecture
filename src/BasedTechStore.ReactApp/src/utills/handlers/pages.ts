// export const handlePageChange = (page: number) => {
//     const newParams = new URLSearchParams(searchParams);
//     newParams.set('page', page.toString());
//     window.history.pushState({}, '', `${window.location.pathname}?${newParams}`);

//     refetch({ ...filters, page });
// };