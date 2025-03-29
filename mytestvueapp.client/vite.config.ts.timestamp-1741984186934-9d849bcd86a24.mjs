// vite.config.ts
import { fileURLToPath, URL } from "node:url";
import { defineConfig } from "file:///C:/Users/helmrickj9306/source/repos/Pixel-Painter-2.0/mytestvueapp.client/node_modules/vite/dist/node/index.js";
import plugin from "file:///C:/Users/helmrickj9306/source/repos/Pixel-Painter-2.0/mytestvueapp.client/node_modules/@vitejs/plugin-vue/dist/index.mjs";
import fs from "fs";
import path from "path";
import child_process from "child_process";
import { env } from "process";
var __vite_injected_original_import_meta_url =
  "file:///C:/Users/helmrickj9306/source/repos/Pixel-Painter-2.0/mytestvueapp.client/vite.config.ts";
var baseFolder =
  env.APPDATA !== void 0 && env.APPDATA !== ""
    ? `${env.APPDATA}/ASP.NET/https`
    : `${env.HOME}/.aspnet/https`;
var certificateName = "mytestvueapp.client";
var certFilePath = path.join(baseFolder, `${certificateName}.pem`);
var keyFilePath = path.join(baseFolder, `${certificateName}.key`);
if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
  if (
    0 !==
    child_process.spawnSync(
      "dotnet",
      [
        "dev-certs",
        "https",
        "--export-path",
        certFilePath,
        "--format",
        "Pem",
        "--no-password"
      ],
      { stdio: "inherit" }
    ).status
  ) {
    throw new Error("Could not create certificate.");
  }
}
var target = env.ASPNETCORE_HTTPS_PORT
  ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}`
  : env.ASPNETCORE_URLS
  ? env.ASPNETCORE_URLS.split(";")[0]
  : "https://localhost:7154";
var vite_config_default = defineConfig({
  plugins: [plugin()],
  resolve: {
    alias: {
      "@": fileURLToPath(
        new URL("./src", __vite_injected_original_import_meta_url)
      )
    }
  },
  server: {
    proxy: {
      "^/artaccess": {
        target,
        secure: false
      },
      "^/comment": {
        target,
        secure: false
      },
      "^/login": {
        target,
        secure: false
      },
      "^/like": {
        target,
        secure: false
      },
      "^/notification/": {
        target,
        secure: false
      }
    },
    port: 5173,
    https: {
      key: fs.readFileSync(keyFilePath),
      cert: fs.readFileSync(certFilePath)
    }
  }
});
export { vite_config_default as default };
//# sourceMappingURL=data:application/json;base64,ewogICJ2ZXJzaW9uIjogMywKICAic291cmNlcyI6IFsidml0ZS5jb25maWcudHMiXSwKICAic291cmNlc0NvbnRlbnQiOiBbImNvbnN0IF9fdml0ZV9pbmplY3RlZF9vcmlnaW5hbF9kaXJuYW1lID0gXCJDOlxcXFxVc2Vyc1xcXFxoZWxtcmlja2o5MzA2XFxcXHNvdXJjZVxcXFxyZXBvc1xcXFxQaXhlbC1QYWludGVyLTIuMFxcXFxteXRlc3R2dWVhcHAuY2xpZW50XCI7Y29uc3QgX192aXRlX2luamVjdGVkX29yaWdpbmFsX2ZpbGVuYW1lID0gXCJDOlxcXFxVc2Vyc1xcXFxoZWxtcmlja2o5MzA2XFxcXHNvdXJjZVxcXFxyZXBvc1xcXFxQaXhlbC1QYWludGVyLTIuMFxcXFxteXRlc3R2dWVhcHAuY2xpZW50XFxcXHZpdGUuY29uZmlnLnRzXCI7Y29uc3QgX192aXRlX2luamVjdGVkX29yaWdpbmFsX2ltcG9ydF9tZXRhX3VybCA9IFwiZmlsZTovLy9DOi9Vc2Vycy9oZWxtcmlja2o5MzA2L3NvdXJjZS9yZXBvcy9QaXhlbC1QYWludGVyLTIuMC9teXRlc3R2dWVhcHAuY2xpZW50L3ZpdGUuY29uZmlnLnRzXCI7aW1wb3J0IHsgZmlsZVVSTFRvUGF0aCwgVVJMIH0gZnJvbSBcIm5vZGU6dXJsXCI7XG5cbmltcG9ydCB7IGRlZmluZUNvbmZpZyB9IGZyb20gXCJ2aXRlXCI7XG5pbXBvcnQgcGx1Z2luIGZyb20gXCJAdml0ZWpzL3BsdWdpbi12dWVcIjtcbmltcG9ydCBmcyBmcm9tIFwiZnNcIjtcbmltcG9ydCBwYXRoIGZyb20gXCJwYXRoXCI7XG5pbXBvcnQgY2hpbGRfcHJvY2VzcyBmcm9tIFwiY2hpbGRfcHJvY2Vzc1wiO1xuaW1wb3J0IHsgZW52IH0gZnJvbSBcInByb2Nlc3NcIjtcblxuY29uc3QgYmFzZUZvbGRlciA9XG4gIGVudi5BUFBEQVRBICE9PSB1bmRlZmluZWQgJiYgZW52LkFQUERBVEEgIT09IFwiXCJcbiAgICA/IGAke2Vudi5BUFBEQVRBfS9BU1AuTkVUL2h0dHBzYFxuICAgIDogYCR7ZW52LkhPTUV9Ly5hc3BuZXQvaHR0cHNgO1xuXG5jb25zdCBjZXJ0aWZpY2F0ZU5hbWUgPSBcIm15dGVzdHZ1ZWFwcC5jbGllbnRcIjtcbmNvbnN0IGNlcnRGaWxlUGF0aCA9IHBhdGguam9pbihiYXNlRm9sZGVyLCBgJHtjZXJ0aWZpY2F0ZU5hbWV9LnBlbWApO1xuY29uc3Qga2V5RmlsZVBhdGggPSBwYXRoLmpvaW4oYmFzZUZvbGRlciwgYCR7Y2VydGlmaWNhdGVOYW1lfS5rZXlgKTtcblxuaWYgKCFmcy5leGlzdHNTeW5jKGNlcnRGaWxlUGF0aCkgfHwgIWZzLmV4aXN0c1N5bmMoa2V5RmlsZVBhdGgpKSB7XG4gIGlmIChcbiAgICAwICE9PVxuICAgIGNoaWxkX3Byb2Nlc3Muc3Bhd25TeW5jKFxuICAgICAgXCJkb3RuZXRcIixcbiAgICAgIFtcbiAgICAgICAgXCJkZXYtY2VydHNcIixcbiAgICAgICAgXCJodHRwc1wiLFxuICAgICAgICBcIi0tZXhwb3J0LXBhdGhcIixcbiAgICAgICAgY2VydEZpbGVQYXRoLFxuICAgICAgICBcIi0tZm9ybWF0XCIsXG4gICAgICAgIFwiUGVtXCIsXG4gICAgICAgIFwiLS1uby1wYXNzd29yZFwiLFxuICAgICAgXSxcbiAgICAgIHsgc3RkaW86IFwiaW5oZXJpdFwiIH0sXG4gICAgKS5zdGF0dXNcbiAgKSB7XG4gICAgdGhyb3cgbmV3IEVycm9yKFwiQ291bGQgbm90IGNyZWF0ZSBjZXJ0aWZpY2F0ZS5cIik7XG4gIH1cbn1cblxuY29uc3QgdGFyZ2V0ID0gZW52LkFTUE5FVENPUkVfSFRUUFNfUE9SVFxuICA/IGBodHRwczovL2xvY2FsaG9zdDoke2Vudi5BU1BORVRDT1JFX0hUVFBTX1BPUlR9YFxuICA6IGVudi5BU1BORVRDT1JFX1VSTFNcbiAgICA/IGVudi5BU1BORVRDT1JFX1VSTFMuc3BsaXQoXCI7XCIpWzBdXG4gICAgOiBcImh0dHBzOi8vbG9jYWxob3N0OjcxNTRcIjtcblxuLy8gaHR0cHM6Ly92aXRlanMuZGV2L2NvbmZpZy9cbmV4cG9ydCBkZWZhdWx0IGRlZmluZUNvbmZpZyh7XG4gIHBsdWdpbnM6IFtwbHVnaW4oKV0sXG4gIHJlc29sdmU6IHtcbiAgICBhbGlhczoge1xuICAgICAgXCJAXCI6IGZpbGVVUkxUb1BhdGgobmV3IFVSTChcIi4vc3JjXCIsIGltcG9ydC5tZXRhLnVybCkpXG4gICAgfVxuICB9LFxuICBzZXJ2ZXI6IHtcbiAgICBwcm94eToge1xuICAgICAgXCJeL2FydGFjY2Vzc1wiOiB7XG4gICAgICAgIHRhcmdldCxcbiAgICAgICAgc2VjdXJlOiBmYWxzZVxuICAgICAgfSxcbiAgICAgIFwiXi9jb21tZW50XCI6IHtcbiAgICAgICAgdGFyZ2V0LFxuICAgICAgICBzZWN1cmU6IGZhbHNlXG4gICAgICB9LFxuICAgICAgXCJeL2xvZ2luXCI6IHtcbiAgICAgICAgdGFyZ2V0LFxuICAgICAgICBzZWN1cmU6IGZhbHNlXG4gICAgICB9LFxuICAgICAgXCJeL2xpa2VcIjoge1xuICAgICAgICB0YXJnZXQsXG4gICAgICAgIHNlY3VyZTogZmFsc2VcbiAgICAgIH0sXG4gICAgICBcIl4vbm90aWZpY2F0aW9uXCI6IHtcbiAgICAgICAgdGFyZ2V0LFxuICAgICAgICBzZWN1cmU6ZmFsc2VcbiAgICAgIH1cbiAgICB9LFxuICAgIHBvcnQ6IDUxNzMsXG4gICAgaHR0cHM6IHtcbiAgICAgIGtleTogZnMucmVhZEZpbGVTeW5jKGtleUZpbGVQYXRoKSxcbiAgICAgIGNlcnQ6IGZzLnJlYWRGaWxlU3luYyhjZXJ0RmlsZVBhdGgpLFxuICAgIH1cbiAgfVxufSk7XG4iXSwKICAibWFwcGluZ3MiOiAiO0FBQTJaLFNBQVMsZUFBZSxXQUFXO0FBRTliLFNBQVMsb0JBQW9CO0FBQzdCLE9BQU8sWUFBWTtBQUNuQixPQUFPLFFBQVE7QUFDZixPQUFPLFVBQVU7QUFDakIsT0FBTyxtQkFBbUI7QUFDMUIsU0FBUyxXQUFXO0FBUG1QLElBQU0sMkNBQTJDO0FBU3hULElBQU0sYUFDSixJQUFJLFlBQVksVUFBYSxJQUFJLFlBQVksS0FDekMsR0FBRyxJQUFJLE9BQU8sbUJBQ2QsR0FBRyxJQUFJLElBQUk7QUFFakIsSUFBTSxrQkFBa0I7QUFDeEIsSUFBTSxlQUFlLEtBQUssS0FBSyxZQUFZLEdBQUcsZUFBZSxNQUFNO0FBQ25FLElBQU0sY0FBYyxLQUFLLEtBQUssWUFBWSxHQUFHLGVBQWUsTUFBTTtBQUVsRSxJQUFJLENBQUMsR0FBRyxXQUFXLFlBQVksS0FBSyxDQUFDLEdBQUcsV0FBVyxXQUFXLEdBQUc7QUFDL0QsTUFDRSxNQUNBLGNBQWM7QUFBQSxJQUNaO0FBQUEsSUFDQTtBQUFBLE1BQ0U7QUFBQSxNQUNBO0FBQUEsTUFDQTtBQUFBLE1BQ0E7QUFBQSxNQUNBO0FBQUEsTUFDQTtBQUFBLE1BQ0E7QUFBQSxJQUNGO0FBQUEsSUFDQSxFQUFFLE9BQU8sVUFBVTtBQUFBLEVBQ3JCLEVBQUUsUUFDRjtBQUNBLFVBQU0sSUFBSSxNQUFNLCtCQUErQjtBQUFBLEVBQ2pEO0FBQ0Y7QUFFQSxJQUFNLFNBQVMsSUFBSSx3QkFDZixxQkFBcUIsSUFBSSxxQkFBcUIsS0FDOUMsSUFBSSxrQkFDRixJQUFJLGdCQUFnQixNQUFNLEdBQUcsRUFBRSxDQUFDLElBQ2hDO0FBR04sSUFBTyxzQkFBUSxhQUFhO0FBQUEsRUFDMUIsU0FBUyxDQUFDLE9BQU8sQ0FBQztBQUFBLEVBQ2xCLFNBQVM7QUFBQSxJQUNQLE9BQU87QUFBQSxNQUNMLEtBQUssY0FBYyxJQUFJLElBQUksU0FBUyx3Q0FBZSxDQUFDO0FBQUEsSUFDdEQ7QUFBQSxFQUNGO0FBQUEsRUFDQSxRQUFRO0FBQUEsSUFDTixPQUFPO0FBQUEsTUFDTCxlQUFlO0FBQUEsUUFDYjtBQUFBLFFBQ0EsUUFBUTtBQUFBLE1BQ1Y7QUFBQSxNQUNBLGFBQWE7QUFBQSxRQUNYO0FBQUEsUUFDQSxRQUFRO0FBQUEsTUFDVjtBQUFBLE1BQ0EsV0FBVztBQUFBLFFBQ1Q7QUFBQSxRQUNBLFFBQVE7QUFBQSxNQUNWO0FBQUEsTUFDQSxVQUFVO0FBQUEsUUFDUjtBQUFBLFFBQ0EsUUFBUTtBQUFBLE1BQ1Y7QUFBQSxNQUNBLGtCQUFrQjtBQUFBLFFBQ2hCO0FBQUEsUUFDQSxRQUFPO0FBQUEsTUFDVDtBQUFBLElBQ0Y7QUFBQSxJQUNBLE1BQU07QUFBQSxJQUNOLE9BQU87QUFBQSxNQUNMLEtBQUssR0FBRyxhQUFhLFdBQVc7QUFBQSxNQUNoQyxNQUFNLEdBQUcsYUFBYSxZQUFZO0FBQUEsSUFDcEM7QUFBQSxFQUNGO0FBQ0YsQ0FBQzsiLAogICJuYW1lcyI6IFtdCn0K

