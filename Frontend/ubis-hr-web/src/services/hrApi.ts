import axios from "axios";

const hrApi = axios.create({
  baseURL: import.meta.env.VITE_HR_API_URL,
  timeout: 15000,
});

hrApi.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) config.headers.Authorization = `Bearer ${token}`;

  config.headers["X-Api-Key"] = import.meta.env.VITE_HR_API_KEY;
  return config;
});

hrApi.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem("token");
      localStorage.removeItem("email");
      localStorage.removeItem("displayName");
      localStorage.removeItem("employeeCode");
      localStorage.removeItem("permissions");

      // ใช้ window.location แทน router เพื่อตัด Circular Dependency
      // (services → router → views → stores → services)
      // Token หมดอายุต้องเคลียร์ State ทั้งหมดอยู่แล้ว การ Reload เต็มหน้าจึงเหมาะสม
      if (window.location.pathname !== "/login") {
        window.location.href = "/login";
      }
    }
    return Promise.reject(error);
  },
);

export default hrApi;
