export default class GIFCreationService {
    public static async createGIF(frames: string[], fps: number) {
        try {
            const response = await fetch("/GIFCreation/CreateGif", {
                method: "POST",
                body: JSON.stringify({ "frames": frames, "fps": fps }),
                headers: { "Content-Type": "application/json" }
            });

            if (!response.ok) {
                console.error("Server error:", response.status, await response.text());
                throw new Error("Failed to create GIF");
            }

            const blob = await response.blob();
            const url = window.URL.createObjectURL(blob);
            const link = document.createElement("a");
            link.href = url;
            link.download = "animation.gif";
            link.click();
        } catch (error) {
            console.error("Error creating GIF:", error);
            throw error;
        }
    }
}