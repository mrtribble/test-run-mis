// Application state and UI management
let shops = [];

// Initialize the application
document.addEventListener('DOMContentLoaded', () => {
    loadShops();
    setupEventListeners();
});

// Setup event listeners
function setupEventListeners() {
    const addShopForm = document.getElementById('addShopForm');
    addShopForm.addEventListener('submit', handleAddShop);
}

// Load all shops from the API
async function loadShops() {
    try {
        shops = await api.getShops();
        renderShops();
    } catch (error) {
        console.error('Error loading shops:', error);
        const errorMessage = error.message || 'Failed to load shops. Please try again.';
        showError(errorMessage);
        renderEmptyState();
    }
}

// Handle add shop form submission
async function handleAddShop(event) {
    event.preventDefault();

    const shopNameInput = document.getElementById('shopName');
    const ratingInput = document.getElementById('rating');

    const shopName = shopNameInput.value.trim();
    const rating = parseFloat(ratingInput.value);

    if (!shopName) {
        showError('Please enter a shop name');
        return;
    }

    if (isNaN(rating) || rating < 0 || rating > 5) {
        showError('Please enter a valid rating between 0 and 5');
        return;
    }

    try {
        await api.addShop({
            shopName: shopName,
            rating: rating
        });

        // Clear form
        shopNameInput.value = '';
        ratingInput.value = '';

        // Reload shops
        await loadShops();
    } catch (error) {
        showError(error.message || 'Failed to add shop. Please try again.');
    }
}

// Handle favorite toggle
async function handleToggleFavorite(shopId) {
    try {
        await api.toggleFavorite(shopId);
        await loadShops();
    } catch (error) {
        showError('Failed to update favorite status. Please try again.');
    }
}

// Handle delete shop
async function handleDeleteShop(shopId) {
    if (!confirm('Are you sure you want to delete this shop?')) {
        return;
    }

    try {
        await api.deleteShop(shopId);
        await loadShops();
    } catch (error) {
        showError('Failed to delete shop. Please try again.');
    }
}

// Render shops in the table
function renderShops() {
    const tbody = document.getElementById('shopsTableBody');

    if (shops.length === 0) {
        tbody.innerHTML = '<tr><td colspan="5" class="text-center">No coffee shops found. Add one to get started!</td></tr>';
        return;
    }

    tbody.innerHTML = shops.map(shop => `
        <tr>
            <td>${escapeHtml(shop.shopName)}</td>
            <td>
                <span class="badge bg-primary rating-badge">${shop.rating.toFixed(1)}</span>
            </td>
            <td>${formatDate(shop.dateEntered)}</td>
            <td>
                <span class="favorite-btn ${shop.favorited ? 'active' : ''}" 
                      onclick="handleToggleFavorite(${shop.shopID})"
                      title="${shop.favorited ? 'Remove from favorites' : 'Add to favorites'}">
                    ${shop.favorited ? '★' : '☆'}
                </span>
            </td>
            <td>
                <button class="btn btn-sm btn-danger delete-btn" 
                        onclick="handleDeleteShop(${shop.shopID})">
                    Delete
                </button>
            </td>
        </tr>
    `).join('');
}

// Render empty state
function renderEmptyState() {
    const tbody = document.getElementById('shopsTableBody');
    tbody.innerHTML = `
        <tr>
            <td colspan="5" class="text-center text-danger">
                <p>Error loading shops.</p>
                <p class="small">Make sure the backend API is running:</p>
                <p class="small"><code>cd api && dotnet run</code></p>
                <p class="small">Then refresh this page.</p>
            </td>
        </tr>
    `;
}

// Format date for display
function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
}

// Escape HTML to prevent XSS
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// Show error message
function showError(message) {
    // Simple alert for now - could be enhanced with a toast notification
    alert(message);
}

// Make functions available globally for onclick handlers
window.handleToggleFavorite = handleToggleFavorite;
window.handleDeleteShop = handleDeleteShop;

