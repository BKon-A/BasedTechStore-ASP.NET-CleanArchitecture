import './App.css'
import { RouterProvider } from 'react-router-dom'
import { router } from './router'
import { HomePage } from './pages/home/HomePage'

const App = () => {
    return <RouterProvider router={router} />
    //return <HomePage />
}

export default App