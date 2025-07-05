import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'
import { BrowserRouter } from 'react-router-dom'

// Suppress Chrome extension errors in development
if (import.meta.env.DEV) {
    // Suppress runtime.lastError warnings
    const originalError = console.error;
    console.error = (...args) => {
        if (
            args.length > 0 &&
            typeof args[0] === 'string' &&
            (args[0].includes('runtime.lastError') ||
                args[0].includes('Could not establish connection') ||
                args[0].includes('Receiving end does not exist'))
        ) {
            // Ignore extension-related errors
            return;
        }
        originalError.apply(console, args);
    };

    // Handle unhandled promise rejections related to extensions
    window.addEventListener('unhandledrejection', (event) => {
        if (
            event.reason &&
            typeof event.reason === 'object' &&
            event.reason.message &&
            (event.reason.message.includes('Extension context invalidated') ||
                event.reason.message.includes('Could not establish connection'))
        ) {
            event.preventDefault();
        }
    });

    // Handle general errors
    window.addEventListener('error', (event) => {
        if (
            event.message &&
            (event.message.includes('runtime.lastError') ||
                event.message.includes('Could not establish connection'))
        ) {
            event.preventDefault();
            return false;
        }
    });
}

const rootElement = document.getElementById('root');

if (!rootElement) {
    throw new Error('Failed to find the root element');
}

createRoot(rootElement).render(
    <StrictMode>
        <App />
    </StrictMode>,
)
