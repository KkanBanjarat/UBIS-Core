import { createApp } from "vue";
import { createPinia, setActivePinia } from "pinia";
import App from "./App.vue";
import router from "./router";
import "./style.css";
import "bootstrap-icons/font/bootstrap-icons.css";
import { useAuthStore } from "./stores/authStore.ts";
// main.ts
const app = createApp(App);
app.use(createPinia());

// รอ MSAL เฉพาะตอนที่กลับมาจาก Microsoft Redirect จริงๆ เท่านั้น
// (URL จะมี #code= หรือ #id_token= ติดมา) กรณีอื่นไม่ต้องรอ Mount ได้เลยทันที
const isMsalRedirect =
  window.location.hash.includes("code=") ||
  window.location.hash.includes("id_token=");

async function bootstrap() {
  if (isMsalRedirect) {
    const { useAuthStore } = await import("./stores/authStore.ts");
    await useAuthStore().handleSsoRedirect();
  }
  app.use(router);
  await router.isReady();
  app.mount("#app");
}

bootstrap();
