// JS Code will be placed here.
// JS code the for the popup when the user enters the web app.
document.addEventListener('DOMContentLoaded', function () {
    var modal = document.getElementById('popupModal');
    if (modal) {
        var closeMsgBtn = modal.querySelector('button.close-button');
        modal.style.display = 'block';
        if (closeMsgBtn) {
            closeMsgBtn.onclick = function () {
                modal.style.display = 'none';
            };
        }
    }

    // Autocomplete for city input
    let cityInput = document.querySelector('input[name="City"]');
    if (!cityInput) {
        cityInput = document.getElementById('City');
    }
    if (!cityInput) {
        cityInput = document.querySelector('input[type="text"]');
    }
    if (!cityInput) {
        console.error('City input not found!');
    }
    const suggestionsBox = document.getElementById('city-suggestions');

    if (cityInput && suggestionsBox) {
        console.log('City input and suggestions box found, autocomplete enabled.');
        cityInput.addEventListener('input', async function () {
            const query = cityInput.value.trim();
            if (query.length < 2) {
                suggestionsBox.innerHTML = '';
                return;
            }
            // Fetch city suggestions via server-side proxy handler to keep API key secret
            const url = `/Index?handler=Geocode&query=${encodeURIComponent(query)}`;
            try {
                const response = await fetch(url);
                const cities = await response.json();
                suggestionsBox.innerHTML = '';
                if (cities.length > 0) {
                    cities.forEach(city => {
                        const item = document.createElement('div');
                        item.className = 'autocomplete-item';
                        item.style.cursor = 'pointer';
                        item.style.background = '#fff';
                        item.style.border = '1px solid #ccc';
                        item.style.padding = '6px';
                        item.style.marginBottom = '2px';
                        const fullString = `${city.name}${city.state ? ', ' + city.state : ''}, ${city.country}`;
                        item.textContent = fullString;
                        item.onclick = function () {
                            cityInput.value = fullString;
                            suggestionsBox.innerHTML = '';
                        };
                        suggestionsBox.appendChild(item);
                    });
                } else {
                    console.log('No city suggestions found for:', query);
                }
            } catch (err) {
                suggestionsBox.innerHTML = '';
                console.error('Error fetching city suggestions:', err);
            }
        });

        // Hide suggestions when clicking outside
        document.addEventListener('click', function (e) {
            if (!suggestionsBox.contains(e.target) && e.target !== cityInput) {
                suggestionsBox.innerHTML = '';
            }
        });
    }
});