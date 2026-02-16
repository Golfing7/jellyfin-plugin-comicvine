// 1. Define the button injection logic
function addBulkTagButton(target, item) {
    // Safety check: Only add if it's a Folder or BoxSet (Collection)
    if (item.Type !== 'Folder' && item.Type !== 'BoxSet') return;

    // Check if button already exists to prevent duplicates
    const header = target.querySelector('.itemsViewSettingsContainer'); // Or '.headerButtonRight' depending on theme
    if (!header || target.querySelector('#cv-bulk-tag-btn')) return;

    const btn = document.createElement('button');
    btn.id = 'cv-bulk-tag-btn';
    btn.type = 'button';
    btn.classList.add('paper-icon-button-light', 'headerButton', 'paper-icon-button');
    btn.innerHTML = '<span class="material-icons">local_offer</span>'; // Ensure you use an icon available in Jellyfin
    btn.title = "Bulk Tag via ComicVine";

    btn.addEventListener('click', async () => {
        // Use the ID we fetched earlier
        const folderId = item.Id;
        const volumeId = prompt("Enter ComicVine Volume ID:");

        if (volumeId) {
            try {
                // Show a loading toast
                Dashboard.alert("Bulk tagging started...");
                Dashboard.showLoadingMsg();

                await ApiClient.fetch({
                    url: ApiClient.getUrl('ComicVine/TagFolder', {
                        folderId: folderId,
                        cvVolumeId: volumeId
                    }),
                    type: 'POST'
                });

                Dashboard.hideLoadingMsg();

                Dashboard.alert("Tagging complete! Refreshing...");
                Events.trigger(document, 'itemReload', { Id: item.Id });
            } catch (err) {
                console.error(err);
                Dashboard.alert({ message: "Error tagging folder. Check console.", title: "Error" });
            }
        }
    });

    header.appendChild(btn);
}

// Observer to detect page navigation (Jellyfin is a SPA)
document.addEventListener('viewshow', function (e) {
    // Debug: See what is actually in the event
    // console.log("Viewshow event:", e.detail);

    // In modern Jellyfin, we often get the ID from the route params passed in e.detail
    // e.detail might look like: { type: "items", params: { parentId: "..." }, ... }

    // The safest fallback is to grab the ID from the current route state
    const currentRouteInfo = e.detail.params || {};
    const itemId = currentRouteInfo ? currentRouteInfo.parentId : null;

    if (itemId) {
        // Fetch the full item details to check if it's a Folder
        ApiClient.getItem(ApiClient.getCurrentUserId(), itemId).then((item) => {
            addBulkTagButton(e.target, item);
        }).catch((err) => {
            console.error("Could not fetch current item details for button injection", err);
        });
    }
});
