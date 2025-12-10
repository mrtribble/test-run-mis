// API configuration
// For local development, use: http://localhost:5000/api or https://localhost:7000/api
// For production, update this to your deployed API URL
// Try HTTP first (port 5000), then HTTPS (port 7000) if HTTP fails
let API_BASE_URL = 'http://localhost:5000/api';

// If opened via file:// protocol, default to localhost
if (window.location.protocol === 'file:' || window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
    API_BASE_URL = 'http://localhost:5000/api';
} else {
    // For production, adjust this URL
    API_BASE_URL = '/api';
}

// API functions
const api = {
    // Get all shops
    async getShops() {
        try {
            const response = await fetch(`${API_BASE_URL}/shop`);
            if (!response.ok) {
                let errorData;
                try {
                    errorData = await response.json();
                } catch {
                    errorData = { message: await response.text() };
                }
                const errorMessage = errorData.details || errorData.message || `Failed to fetch shops: ${response.status} ${response.statusText}`;
                throw new Error(errorMessage);
            }
            return await response.json();
        } catch (error) {
            console.error('Error fetching shops:', error);
            // Provide more helpful error message
            if (error.message.includes('Failed to fetch') || error.message.includes('NetworkError')) {
                throw new Error('Cannot connect to API. Make sure the backend server is running on http://localhost:5000');
            }
            throw error;
        }
    },

    // Add a new shop
    async addShop(shop) {
        try {
            const response = await fetch(`${API_BASE_URL}/shop`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(shop),
            });
            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || 'Failed to add shop');
            }
            return await response.json();
        } catch (error) {
            console.error('Error adding shop:', error);
            throw error;
        }
    },

    // Toggle favorite status
    async toggleFavorite(shopId) {
        try {
            const response = await fetch(`${API_BASE_URL}/shop/${shopId}/favorite`, {
                method: 'PUT',
            });
            if (!response.ok) {
                throw new Error('Failed to update favorite status');
            }
            return await response.json();
        } catch (error) {
            console.error('Error toggling favorite:', error);
            throw error;
        }
    },

    // Delete a shop (soft delete)
    async deleteShop(shopId) {
        try {
            const response = await fetch(`${API_BASE_URL}/shop/${shopId}`, {
                method: 'DELETE',
            });
            if (!response.ok) {
                throw new Error('Failed to delete shop');
            }
            return true;
        } catch (error) {
            console.error('Error deleting shop:', error);
            throw error;
        }
    }
};

