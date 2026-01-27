import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";

const features = [
  {
    title: "Talking Head AI",
    description: "Tạo video nhân vật nói từ ảnh + giọng nói",
    icon: "🎭",
    badge: "LivePortrait",
  },
  {
    title: "Virtual Try-On",
    description: "Thử đồ ảo - mặc quần áo lên ảnh mẫu",
    icon: "👕",
    badge: "IDM-VTON",
  },
  {
    title: "Image to Video",
    description: "Biến ảnh tĩnh thành video động",
    icon: "🎬",
    badge: "SVD-XT",
  },
  {
    title: "Motion Transfer",
    description: "Chuyển chuyển động từ video mẫu",
    icon: "💃",
    badge: "MimicMotion",
  },
  {
    title: "Face Swap",
    description: "Đổi mặt trong video chất lượng cao",
    icon: "🎭",
    badge: "InsightFace",
  },
];

export default function Home() {
  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-900 via-purple-900 to-slate-900">
      {/* Navigation */}
      <nav className="border-b border-white/10 backdrop-blur-sm">
        <div className="container mx-auto px-4 py-4 flex items-center justify-between">
          <div className="flex items-center gap-2">
            <span className="text-2xl">🤖</span>
            <span className="text-xl font-bold text-white">Trợ Lý KOC</span>
          </div>
          <div className="flex items-center gap-4">
            <Link href="/login">
              <Button variant="ghost" className="text-white hover:text-white hover:bg-white/10">
                Đăng nhập
              </Button>
            </Link>
            <Link href="/register">
              <Button className="bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700">
                Đăng ký miễn phí
              </Button>
            </Link>
          </div>
        </div>
      </nav>

      {/* Hero Section */}
      <section className="container mx-auto px-4 py-20 text-center">
        <Badge variant="secondary" className="mb-4 bg-purple-500/20 text-purple-300 border-purple-500/30">
          🚀 Powered by AI
        </Badge>
        <h1 className="text-5xl md:text-7xl font-bold text-white mb-6 leading-tight">
          Tạo Video AI<br />
          <span className="text-transparent bg-clip-text bg-gradient-to-r from-purple-400 to-pink-400">
            Chỉ Với Vài Click
          </span>
        </h1>
        <p className="text-xl text-gray-300 mb-8 max-w-2xl mx-auto">
          Nền tảng AI video cho KOL/KOC Việt Nam. Tạo Talking Head, thử đồ ảo,
          đổi mặt trong video và nhiều hơn nữa.
        </p>
        <div className="flex items-center justify-center gap-4">
          <Link href="/register">
            <Button size="lg" className="bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700 text-lg px-8">
              Bắt Đầu Miễn Phí
            </Button>
          </Link>
          <Link href="/pricing">
            <Button size="lg" variant="outline" className="text-white border-white/30 hover:bg-white/10 text-lg px-8">
              Xem Bảng Giá
            </Button>
          </Link>
        </div>
      </section>

      {/* Features Section */}
      <section className="container mx-auto px-4 py-20">
        <h2 className="text-3xl font-bold text-white text-center mb-12">
          5 Công Cụ AI Mạnh Mẽ
        </h2>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {features.map((feature, index) => (
            <Card key={index} className="bg-white/5 border-white/10 hover:bg-white/10 transition-colors">
              <CardHeader>
                <div className="flex items-center gap-3">
                  <span className="text-4xl">{feature.icon}</span>
                  <div>
                    <CardTitle className="text-white">{feature.title}</CardTitle>
                    <Badge variant="outline" className="mt-1 text-purple-300 border-purple-500/30">
                      {feature.badge}
                    </Badge>
                  </div>
                </div>
              </CardHeader>
              <CardContent>
                <CardDescription className="text-gray-400 text-base">
                  {feature.description}
                </CardDescription>
              </CardContent>
            </Card>
          ))}
        </div>
      </section>

      {/* CTA Section */}
      <section className="container mx-auto px-4 py-20 text-center">
        <Card className="bg-gradient-to-r from-purple-600/20 to-pink-600/20 border-purple-500/30 p-8">
          <CardHeader>
            <CardTitle className="text-3xl text-white">Sẵn Sàng Tạo Video AI?</CardTitle>
            <CardDescription className="text-gray-300 text-lg">
              Đăng ký ngay và nhận 10 lượt render miễn phí
            </CardDescription>
          </CardHeader>
          <CardContent>
            <Link href="/register">
              <Button size="lg" className="bg-white text-purple-900 hover:bg-gray-100 text-lg px-8">
                Bắt Đầu Ngay
              </Button>
            </Link>
          </CardContent>
        </Card>
      </section>

      {/* Footer */}
      <footer className="border-t border-white/10 py-8">
        <div className="container mx-auto px-4 text-center text-gray-400">
          <p>© 2026 Trợ Lý KOC. Made with ❤️ in Vietnam</p>
        </div>
      </footer>
    </div>
  );
}
