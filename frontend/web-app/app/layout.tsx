import type { Metadata } from "next";
import "./globals.css";
import Navbar from "./nav/Navbar";
import { ThemeModeScript } from "flowbite-react";

export const metadata: Metadata = {
  title: "Fabio Carsties app",
  description: "Generated by create next app",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <head>
        <ThemeModeScript />
      </head>
      <body>
        <Navbar />
        <main className="container mx-auto px-5 pt-10">
          {children}
        </main>
      </body>
    </html>
  );
}
