const apiBaseUrl = import.meta.env.VITE_TESTING_MODE
  ? import.meta.env.VITE_LOCAL_API_URL
  : import.meta.env.VITE_PRODUCTION_API_URL;

function getApiUrl(path) {
  if (!apiBaseUrl) {
    throw new Error("The API URL is not configured.");
  }

  return `${apiBaseUrl.replace(/\/$/, "")}${path}`;
}

export async function get(path, options = {}) {
  const response = await fetch(getApiUrl(path), options);

  if (!response.ok) {
    throw new Error("Unable to load item data.");
  }

  if (response.status === 204) {
    return null;
  }

  return response.json();
}
