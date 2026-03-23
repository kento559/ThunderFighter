Add-Type -AssemblyName System.Drawing

$root = 'D:\code\Game Project\ThunderFighterUnity\Assets\Resources\GeneratedArt'
New-Item -ItemType Directory -Force -Path $root | Out-Null

function New-Bitmap($w, $h) {
    $bmp = New-Object System.Drawing.Bitmap($w, $h, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
    return @{ Bitmap = $bmp; Graphics = $g }
}

function Save-Bitmap($ctx, $path) {
    $ctx.Graphics.Dispose()
    $ctx.Bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $ctx.Bitmap.Dispose()
}

function New-Path($points) {
    $gp = New-Object System.Drawing.Drawing2D.GraphicsPath
    $gp.AddPolygon($points)
    return $gp
}

function Fill-PathGradient($g, $path, $centerColor, $surroundColor) {
    $brush = New-Object System.Drawing.Drawing2D.PathGradientBrush($path)
    $brush.CenterColor = $centerColor
    $brush.SurroundColors = [System.Drawing.Color[]]@($surroundColor)
    $g.FillPath($brush, $path)
    $brush.Dispose()
}

function Fill-Linear($g, $rect, $c1, $c2, $angle, $scriptBlock) {
    $brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush($rect, $c1, $c2, $angle)
    & $scriptBlock $brush
    $brush.Dispose()
}

function New-Pen($color, $width) {
    if ($color -is [System.Array]) {
        $color = $color[0]
    }

    $pen = New-Object System.Drawing.Pen($color, $width)
    $pen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
    return $pen
}

function Draw-HexPanel($g, $x, $y, $w, $h, $fill1, $fill2, $outlineColor) {
    $points = [System.Drawing.PointF[]]@(
        [System.Drawing.PointF]::new($x + $w * 0.5, $y),
        [System.Drawing.PointF]::new($x + $w, $y + $h * 0.22),
        [System.Drawing.PointF]::new($x + $w * 0.9, $y + $h),
        [System.Drawing.PointF]::new($x + $w * 0.1, $y + $h),
        [System.Drawing.PointF]::new($x, $y + $h * 0.22)
    )
    $path = New-Path $points
    Fill-Linear $g ([System.Drawing.RectangleF]::new($x, $y, $w, $h)) $fill1 $fill2 90 { param($brush) $g.FillPath($brush, $path) }
    $pen = New-Pen $outlineColor 3
    $g.DrawPath($pen, $path)
    $pen.Dispose()
    $path.Dispose()
}

function Draw-EngineGlow($g, $x, $y, $w, $h, $core, $outer) {
    $rect = [System.Drawing.RectangleF]::new($x, $y, $w, $h)
    Fill-Linear $g $rect $outer $core 90 { param($brush) $g.FillEllipse($brush, $rect) }
    $shine = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(130,255,255,255))
    $g.FillEllipse($shine, $x + $w * 0.28, $y + $h * 0.18, $w * 0.24, $h * 0.46)
    $shine.Dispose()
}

function Draw-PlayerShip($path) {
    $ctx = New-Bitmap 1024 1024
    $g = $ctx.Graphics
    $g.Clear([System.Drawing.Color]::Transparent)

    $shadowBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(70, 0, 0, 0))
    $g.FillEllipse($shadowBrush, 190, 728, 644, 170)
    $shadowBrush.Dispose()

    $wingLeft = New-Path ([System.Drawing.PointF[]]@(
        [System.Drawing.PointF]::new(422, 336),
        [System.Drawing.PointF]::new(102, 512),
        [System.Drawing.PointF]::new(176, 674),
        [System.Drawing.PointF]::new(398, 598),
        [System.Drawing.PointF]::new(464, 430)
    ))
    $wingRight = New-Path ([System.Drawing.PointF[]]@(
        [System.Drawing.PointF]::new(602, 336),
        [System.Drawing.PointF]::new(922, 512),
        [System.Drawing.PointF]::new(848, 674),
        [System.Drawing.PointF]::new(626, 598),
        [System.Drawing.PointF]::new(560, 430)
    ))
    $body = New-Path ([System.Drawing.PointF[]]@(
        [System.Drawing.PointF]::new(512, 78),
        [System.Drawing.PointF]::new(584, 188),
        [System.Drawing.PointF]::new(564, 460),
        [System.Drawing.PointF]::new(636, 828),
        [System.Drawing.PointF]::new(512, 942),
        [System.Drawing.PointF]::new(388, 828),
        [System.Drawing.PointF]::new(460, 460),
        [System.Drawing.PointF]::new(440, 188)
    ))
    $podLeft = New-Path ([System.Drawing.PointF[]]@(
        [System.Drawing.PointF]::new(342, 432),
        [System.Drawing.PointF]::new(258, 496),
        [System.Drawing.PointF]::new(248, 702),
        [System.Drawing.PointF]::new(338, 760),
        [System.Drawing.PointF]::new(406, 690),
        [System.Drawing.PointF]::new(426, 520)
    ))
    $podRight = New-Path ([System.Drawing.PointF[]]@(
        [System.Drawing.PointF]::new(682, 432),
        [System.Drawing.PointF]::new(766, 496),
        [System.Drawing.PointF]::new(776, 702),
        [System.Drawing.PointF]::new(686, 760),
        [System.Drawing.PointF]::new(618, 690),
        [System.Drawing.PointF]::new(598, 520)
    ))

    Fill-Linear $g ([System.Drawing.RectangleF]::new(90,330,380,350)) ([System.Drawing.Color]::FromArgb(255, 56, 98, 130)) ([System.Drawing.Color]::FromArgb(255, 154, 189, 214)) 90 { param($brush) $g.FillPath($brush, $wingLeft) }
    Fill-Linear $g ([System.Drawing.RectangleF]::new(554,330,380,350)) ([System.Drawing.Color]::FromArgb(255, 56, 98, 130)) ([System.Drawing.Color]::FromArgb(255, 154, 189, 214)) 90 { param($brush) $g.FillPath($brush, $wingRight) }
    Fill-Linear $g ([System.Drawing.RectangleF]::new(384,80,260,860)) ([System.Drawing.Color]::FromArgb(255, 72, 88, 102)) ([System.Drawing.Color]::FromArgb(255, 210, 219, 226)) 90 { param($brush) $g.FillPath($brush, $body) }
    Fill-Linear $g ([System.Drawing.RectangleF]::new(246,430,200,330)) ([System.Drawing.Color]::FromArgb(255, 42, 78, 104)) ([System.Drawing.Color]::FromArgb(255, 116, 170, 202)) 90 { param($brush) $g.FillPath($brush, $podLeft) }
    Fill-Linear $g ([System.Drawing.RectangleF]::new(578,430,200,330)) ([System.Drawing.Color]::FromArgb(255, 42, 78, 104)) ([System.Drawing.Color]::FromArgb(255, 116, 170, 202)) 90 { param($brush) $g.FillPath($brush, $podRight) }

    $outline = New-Pen ([System.Drawing.Color]::FromArgb(230, 230, 240, 245)) 10
    $g.DrawPath($outline, $wingLeft)
    $g.DrawPath($outline, $wingRight)
    $g.DrawPath($outline, $body)
    $g.DrawPath($outline, $podLeft)
    $g.DrawPath($outline, $podRight)
    $outline.Dispose()

    Draw-HexPanel $g 470 150 84 118 ([System.Drawing.Color]::FromArgb(255, 240, 247, 252)) ([System.Drawing.Color]::FromArgb(255, 74, 201, 233)) ([System.Drawing.Color]::FromArgb(220, 214, 248, 255))
    Draw-HexPanel $g 482 294 60 180 ([System.Drawing.Color]::FromArgb(255, 212, 223, 228)) ([System.Drawing.Color]::FromArgb(255, 92, 120, 144)) ([System.Drawing.Color]::FromArgb(160, 228, 240, 244))
    Draw-HexPanel $g 432 458 160 162 ([System.Drawing.Color]::FromArgb(255, 194, 204, 210)) ([System.Drawing.Color]::FromArgb(255, 88, 102, 116)) ([System.Drawing.Color]::FromArgb(160, 224, 234, 240))

    $linePen = New-Pen ([System.Drawing.Color]::FromArgb(175, 245, 252, 255)) 5
    $g.DrawLine($linePen, 512, 112, 512, 848)
    $g.DrawLine($linePen, 298, 594, 404, 564)
    $g.DrawLine($linePen, 726, 594, 620, 564)
    $linePen.Dispose()

    $accentPen = New-Pen ([System.Drawing.Color]::FromArgb(160, 120, 232, 255)) 6
    $g.DrawLine($accentPen, 266, 666, 346, 716)
    $g.DrawLine($accentPen, 758, 666, 678, 716)
    $accentPen.Dispose()

    Draw-EngineGlow $g 430 824 54 102 ([System.Drawing.Color]::FromArgb(255, 255, 222, 164)) ([System.Drawing.Color]::FromArgb(255, 255, 110, 26))
    Draw-EngineGlow $g 540 824 54 102 ([System.Drawing.Color]::FromArgb(255, 255, 222, 164)) ([System.Drawing.Color]::FromArgb(255, 255, 110, 26))

    $wingLeft.Dispose()
    $wingRight.Dispose()
    $body.Dispose()
    $podLeft.Dispose()
    $podRight.Dispose()

    Save-Bitmap $ctx $path
}

function Draw-PlayerShipVariant([string]$path, [string]$variant) {
    $ctx = New-Bitmap 1024 1024
    $g = $ctx.Graphics
    $g.Clear([System.Drawing.Color]::Transparent)

    $shadowBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(74, 0, 0, 0))
    $g.FillEllipse($shadowBrush, 174, 730, 676, 168)
    $shadowBrush.Dispose()

    switch ($variant) {
        'Rapid' {
            $wingLeft = New-Path ([System.Drawing.PointF[]]@(
                [System.Drawing.PointF]::new(446, 278),
                [System.Drawing.PointF]::new(88, 474),
                [System.Drawing.PointF]::new(160, 660),
                [System.Drawing.PointF]::new(404, 604),
                [System.Drawing.PointF]::new(476, 410)
            ))
            $wingRight = New-Path ([System.Drawing.PointF[]]@(
                [System.Drawing.PointF]::new(578, 278),
                [System.Drawing.PointF]::new(936, 474),
                [System.Drawing.PointF]::new(864, 660),
                [System.Drawing.PointF]::new(620, 604),
                [System.Drawing.PointF]::new(548, 410)
            ))
            $body = New-Path ([System.Drawing.PointF[]]@(
                [System.Drawing.PointF]::new(512, 56),
                [System.Drawing.PointF]::new(566, 166),
                [System.Drawing.PointF]::new(548, 432),
                [System.Drawing.PointF]::new(610, 842),
                [System.Drawing.PointF]::new(512, 956),
                [System.Drawing.PointF]::new(414, 842),
                [System.Drawing.PointF]::new(476, 432),
                [System.Drawing.PointF]::new(458, 166)
            ))
            $leftPod = New-Path ([System.Drawing.PointF[]]@(
                [System.Drawing.PointF]::new(348, 394),
                [System.Drawing.PointF]::new(270, 454),
                [System.Drawing.PointF]::new(262, 674),
                [System.Drawing.PointF]::new(336, 720),
                [System.Drawing.PointF]::new(396, 662),
                [System.Drawing.PointF]::new(414, 476)
            ))
            $rightPod = New-Path ([System.Drawing.PointF[]]@(
                [System.Drawing.PointF]::new(676, 394),
                [System.Drawing.PointF]::new(754, 454),
                [System.Drawing.PointF]::new(762, 674),
                [System.Drawing.PointF]::new(688, 720),
                [System.Drawing.PointF]::new(628, 662),
                [System.Drawing.PointF]::new(610, 476)
            ))

            $wingDark = [System.Drawing.Color]::FromArgb(255, 34, 36, 42)
            $wingLight = [System.Drawing.Color]::FromArgb(255, 124, 94, 58)
            $bodyDark = [System.Drawing.Color]::FromArgb(255, 40, 42, 48)
            $bodyLight = [System.Drawing.Color]::FromArgb(255, 176, 148, 104)
            $accentA = [System.Drawing.Color]::FromArgb(255, 236, 184, 92)
            $accentB = [System.Drawing.Color]::FromArgb(255, 94, 154, 198)
        }
        'Heavy' {
            $wingLeft = New-Path ([System.Drawing.PointF[]]@(
                [System.Drawing.PointF]::new(420, 330),
                [System.Drawing.PointF]::new(74, 466),
                [System.Drawing.PointF]::new(150, 706),
                [System.Drawing.PointF]::new(396, 646),
                [System.Drawing.PointF]::new(480, 484)
            ))
            $wingRight = New-Path ([System.Drawing.PointF[]]@(
                [System.Drawing.PointF]::new(604, 330),
                [System.Drawing.PointF]::new(950, 466),
                [System.Drawing.PointF]::new(874, 706),
                [System.Drawing.PointF]::new(628, 646),
                [System.Drawing.PointF]::new(544, 484)
            ))
            $body = New-Path ([System.Drawing.PointF[]]@(
                [System.Drawing.PointF]::new(512, 82),
                [System.Drawing.PointF]::new(604, 190),
                [System.Drawing.PointF]::new(584, 470),
                [System.Drawing.PointF]::new(658, 864),
                [System.Drawing.PointF]::new(512, 968),
                [System.Drawing.PointF]::new(366, 864),
                [System.Drawing.PointF]::new(440, 470),
                [System.Drawing.PointF]::new(420, 190)
            ))
            $leftPod = New-Path ([System.Drawing.PointF[]]@(
                [System.Drawing.PointF]::new(312, 398),
                [System.Drawing.PointF]::new(218, 472),
                [System.Drawing.PointF]::new(216, 736),
                [System.Drawing.PointF]::new(318, 800),
                [System.Drawing.PointF]::new(412, 726),
                [System.Drawing.PointF]::new(428, 482)
            ))
            $rightPod = New-Path ([System.Drawing.PointF[]]@(
                [System.Drawing.PointF]::new(712, 398),
                [System.Drawing.PointF]::new(806, 472),
                [System.Drawing.PointF]::new(808, 736),
                [System.Drawing.PointF]::new(706, 800),
                [System.Drawing.PointF]::new(612, 726),
                [System.Drawing.PointF]::new(596, 482)
            ))

            $wingDark = [System.Drawing.Color]::FromArgb(255, 54, 40, 28)
            $wingLight = [System.Drawing.Color]::FromArgb(255, 154, 112, 62)
            $bodyDark = [System.Drawing.Color]::FromArgb(255, 50, 50, 54)
            $bodyLight = [System.Drawing.Color]::FromArgb(255, 188, 154, 94)
            $accentA = [System.Drawing.Color]::FromArgb(255, 244, 186, 88)
            $accentB = [System.Drawing.Color]::FromArgb(255, 238, 122, 48)
        }
        default {
            $wingLeft = New-Path ([System.Drawing.PointF[]]@(
                [System.Drawing.PointF]::new(430, 312),
                [System.Drawing.PointF]::new(100, 490),
                [System.Drawing.PointF]::new(176, 682),
                [System.Drawing.PointF]::new(404, 616),
                [System.Drawing.PointF]::new(472, 430)
            ))
            $wingRight = New-Path ([System.Drawing.PointF[]]@(
                [System.Drawing.PointF]::new(594, 312),
                [System.Drawing.PointF]::new(924, 490),
                [System.Drawing.PointF]::new(848, 682),
                [System.Drawing.PointF]::new(620, 616),
                [System.Drawing.PointF]::new(552, 430)
            ))
            $body = New-Path ([System.Drawing.PointF[]]@(
                [System.Drawing.PointF]::new(512, 70),
                [System.Drawing.PointF]::new(586, 184),
                [System.Drawing.PointF]::new(566, 452),
                [System.Drawing.PointF]::new(632, 850),
                [System.Drawing.PointF]::new(512, 956),
                [System.Drawing.PointF]::new(392, 850),
                [System.Drawing.PointF]::new(458, 452),
                [System.Drawing.PointF]::new(438, 184)
            ))
            $leftPod = New-Path ([System.Drawing.PointF[]]@(
                [System.Drawing.PointF]::new(332, 414),
                [System.Drawing.PointF]::new(250, 482),
                [System.Drawing.PointF]::new(244, 716),
                [System.Drawing.PointF]::new(330, 774),
                [System.Drawing.PointF]::new(408, 702),
                [System.Drawing.PointF]::new(424, 500)
            ))
            $rightPod = New-Path ([System.Drawing.PointF[]]@(
                [System.Drawing.PointF]::new(692, 414),
                [System.Drawing.PointF]::new(774, 482),
                [System.Drawing.PointF]::new(780, 716),
                [System.Drawing.PointF]::new(694, 774),
                [System.Drawing.PointF]::new(616, 702),
                [System.Drawing.PointF]::new(600, 500)
            ))

            $wingDark = [System.Drawing.Color]::FromArgb(255, 44, 52, 60)
            $wingLight = [System.Drawing.Color]::FromArgb(255, 118, 136, 148)
            $bodyDark = [System.Drawing.Color]::FromArgb(255, 52, 58, 66)
            $bodyLight = [System.Drawing.Color]::FromArgb(255, 186, 186, 178)
            $accentA = [System.Drawing.Color]::FromArgb(255, 214, 224, 214)
            $accentB = [System.Drawing.Color]::FromArgb(255, 92, 166, 188)
        }
    }

    Fill-Linear $g ([System.Drawing.RectangleF]::new(90,280,390,440)) $wingDark $wingLight 90 { param($brush) $g.FillPath($brush, $wingLeft) }
    Fill-Linear $g ([System.Drawing.RectangleF]::new(544,280,390,440)) $wingDark $wingLight 90 { param($brush) $g.FillPath($brush, $wingRight) }
    Fill-Linear $g ([System.Drawing.RectangleF]::new(340,68,340,900)) $bodyDark $bodyLight 90 { param($brush) $g.FillPath($brush, $body) }
    Fill-Linear $g ([System.Drawing.RectangleF]::new(214,398,230,406)) ([System.Drawing.Color]::FromArgb(255, 34, 42, 50)) $wingLight 90 { param($brush) $g.FillPath($brush, $leftPod) }
    Fill-Linear $g ([System.Drawing.RectangleF]::new(580,398,230,406)) ([System.Drawing.Color]::FromArgb(255, 34, 42, 50)) $wingLight 90 { param($brush) $g.FillPath($brush, $rightPod) }

    $outline = New-Pen ([System.Drawing.Color]::FromArgb(232, 244, 228, 204)) 10
    $g.DrawPath($outline, $wingLeft)
    $g.DrawPath($outline, $wingRight)
    $g.DrawPath($outline, $body)
    $g.DrawPath($outline, $leftPod)
    $g.DrawPath($outline, $rightPod)
    $outline.Dispose()

    Draw-HexPanel $g 466 154 92 124 ([System.Drawing.Color]::FromArgb(255, 250, 236, 210)) $accentA ([System.Drawing.Color]::FromArgb(220, 246, 228, 204))
    Draw-HexPanel $g 454 312 116 178 ([System.Drawing.Color]::FromArgb(255, 136, 140, 142)) ([System.Drawing.Color]::FromArgb(255, 70, 78, 88)) ([System.Drawing.Color]::FromArgb(160, 226, 216, 204))
    Draw-HexPanel $g 414 492 196 176 ([System.Drawing.Color]::FromArgb(255, 108, 116, 124)) ([System.Drawing.Color]::FromArgb(255, 58, 66, 76)) ([System.Drawing.Color]::FromArgb(160, 218, 208, 196))

    $gridPen = New-Pen ([System.Drawing.Color]::FromArgb(136, 228, 214, 194)) 5
    $g.DrawLine($gridPen, 512, 104, 512, 874)
    $g.DrawLine($gridPen, 286, 610, 424, 572)
    $g.DrawLine($gridPen, 738, 610, 600, 572)
    $g.DrawLine($gridPen, 250, 430, 426, 476)
    $g.DrawLine($gridPen, 774, 430, 598, 476)
    $gridPen.Dispose()

    $accentPen = New-Pen ([System.Drawing.Color]::FromArgb(174, $accentB.R, $accentB.G, $accentB.B)) 6
    $g.DrawLine($accentPen, 294, 702, 352, 744)
    $g.DrawLine($accentPen, 730, 702, 672, 744)
    $g.DrawLine($accentPen, 434, 224, 492, 194)
    $g.DrawLine($accentPen, 590, 194, 648, 224)
    $accentPen.Dispose()

    Draw-EngineGlow $g 430 832 54 106 ([System.Drawing.Color]::FromArgb(255, 255, 226, 170)) ([System.Drawing.Color]::FromArgb(255, 255, 118, 34))
    Draw-EngineGlow $g 540 832 54 106 ([System.Drawing.Color]::FromArgb(255, 255, 226, 170)) ([System.Drawing.Color]::FromArgb(255, 255, 118, 34))

    $wingLeft.Dispose()
    $wingRight.Dispose()
    $body.Dispose()
    $leftPod.Dispose()
    $rightPod.Dispose()

    Save-Bitmap $ctx $path
}

function Draw-EnemyShip($path) {
    $ctx = New-Bitmap 1024 1024
    $g = $ctx.Graphics
    $g.Clear([System.Drawing.Color]::Transparent)

    $shadowBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(72, 0, 0, 0))
    $g.FillEllipse($shadowBrush, 206, 144, 612, 158)
    $shadowBrush.Dispose()

    $wingLeft = New-Path ([System.Drawing.PointF[]]@(
        [System.Drawing.PointF]::new(428, 668),
        [System.Drawing.PointF]::new(106, 594),
        [System.Drawing.PointF]::new(186, 324),
        [System.Drawing.PointF]::new(410, 412),
        [System.Drawing.PointF]::new(470, 556)
    ))
    $wingRight = New-Path ([System.Drawing.PointF[]]@(
        [System.Drawing.PointF]::new(596, 668),
        [System.Drawing.PointF]::new(918, 594),
        [System.Drawing.PointF]::new(838, 324),
        [System.Drawing.PointF]::new(614, 412),
        [System.Drawing.PointF]::new(554, 556)
    ))
    $body = New-Path ([System.Drawing.PointF[]]@(
        [System.Drawing.PointF]::new(512, 932),
        [System.Drawing.PointF]::new(610, 804),
        [System.Drawing.PointF]::new(580, 526),
        [System.Drawing.PointF]::new(676, 198),
        [System.Drawing.PointF]::new(512, 90),
        [System.Drawing.PointF]::new(348, 198),
        [System.Drawing.PointF]::new(444, 526),
        [System.Drawing.PointF]::new(414, 804)
    ))

    Fill-Linear $g ([System.Drawing.RectangleF]::new(100,350,380,360)) ([System.Drawing.Color]::FromArgb(255, 36, 20, 26)) ([System.Drawing.Color]::FromArgb(255, 144, 56, 48)) 90 { param($brush) $g.FillPath($brush, $wingLeft) }
    Fill-Linear $g ([System.Drawing.RectangleF]::new(544,350,380,360)) ([System.Drawing.Color]::FromArgb(255, 36, 20, 26)) ([System.Drawing.Color]::FromArgb(255, 144, 56, 48)) 90 { param($brush) $g.FillPath($brush, $wingRight) }
    Fill-Linear $g ([System.Drawing.RectangleF]::new(340,90,340,850)) ([System.Drawing.Color]::FromArgb(255, 40, 42, 48)) ([System.Drawing.Color]::FromArgb(255, 162, 72, 64)) 90 { param($brush) $g.FillPath($brush, $body) }

    $outline = New-Pen ([System.Drawing.Color]::FromArgb(228, 250, 206, 198)) 10
    $g.DrawPath($outline, $wingLeft)
    $g.DrawPath($outline, $wingRight)
    $g.DrawPath($outline, $body)
    $outline.Dispose()

    Draw-HexPanel $g 466 722 92 132 ([System.Drawing.Color]::FromArgb(255, 255, 246, 226)) ([System.Drawing.Color]::FromArgb(255, 255, 128, 64)) ([System.Drawing.Color]::FromArgb(220, 255, 226, 196))
    Draw-HexPanel $g 448 494 128 170 ([System.Drawing.Color]::FromArgb(255, 154, 88, 84)) ([System.Drawing.Color]::FromArgb(255, 66, 52, 60)) ([System.Drawing.Color]::FromArgb(160, 248, 212, 202))
    Draw-HexPanel $g 404 318 216 164 ([System.Drawing.Color]::FromArgb(255, 110, 76, 78)) ([System.Drawing.Color]::FromArgb(255, 58, 42, 46)) ([System.Drawing.Color]::FromArgb(160, 232, 194, 188))

    $clawPen = New-Pen ([System.Drawing.Color]::FromArgb(180, 255, 164, 140)) 7
    $g.DrawLine($clawPen, 214, 478, 416, 448)
    $g.DrawLine($clawPen, 810, 478, 608, 448)
    $clawPen.Dispose()

    $panelPen = New-Pen ([System.Drawing.Color]::FromArgb(135, 244, 215, 210)) 5
    $g.DrawLine($panelPen, 512, 878, 512, 184)
    $g.DrawLine($panelPen, 286, 614, 430, 572)
    $g.DrawLine($panelPen, 738, 614, 594, 572)
    $g.DrawLine($panelPen, 248, 360, 420, 420)
    $g.DrawLine($panelPen, 776, 360, 604, 420)
    $panelPen.Dispose()

    Draw-EngineGlow $g 432 818 60 104 ([System.Drawing.Color]::FromArgb(255, 255, 214, 158)) ([System.Drawing.Color]::FromArgb(255, 255, 92, 24))
    Draw-EngineGlow $g 532 818 60 104 ([System.Drawing.Color]::FromArgb(255, 255, 214, 158)) ([System.Drawing.Color]::FromArgb(255, 255, 92, 24))

    $wingLeft.Dispose()
    $wingRight.Dispose()
    $body.Dispose()

    Save-Bitmap $ctx $path
}

function Draw-BossShip($path) {
    $ctx = New-Bitmap 1280 1280
    $g = $ctx.Graphics
    $g.Clear([System.Drawing.Color]::Transparent)

    $shadowBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(74, 0, 0, 0))
    $g.FillEllipse($shadowBrush, 210, 128, 860, 198)
    $shadowBrush.Dispose()

    $wingLeft = New-Path ([System.Drawing.PointF[]]@(
        [System.Drawing.PointF]::new(522, 868),
        [System.Drawing.PointF]::new(78, 784),
        [System.Drawing.PointF]::new(172, 454),
        [System.Drawing.PointF]::new(478, 522),
        [System.Drawing.PointF]::new(582, 702)
    ))
    $wingRight = New-Path ([System.Drawing.PointF[]]@(
        [System.Drawing.PointF]::new(758, 868),
        [System.Drawing.PointF]::new(1202, 784),
        [System.Drawing.PointF]::new(1108, 454),
        [System.Drawing.PointF]::new(802, 522),
        [System.Drawing.PointF]::new(698, 702)
    ))
    $body = New-Path ([System.Drawing.PointF[]]@(
        [System.Drawing.PointF]::new(640, 1170),
        [System.Drawing.PointF]::new(782, 1016),
        [System.Drawing.PointF]::new(742, 634),
        [System.Drawing.PointF]::new(868, 220),
        [System.Drawing.PointF]::new(640, 86),
        [System.Drawing.PointF]::new(412, 220),
        [System.Drawing.PointF]::new(538, 634),
        [System.Drawing.PointF]::new(498, 1016)
    ))
    $crown = New-Path ([System.Drawing.PointF[]]@(
        [System.Drawing.PointF]::new(640, 1236),
        [System.Drawing.PointF]::new(736, 1126),
        [System.Drawing.PointF]::new(692, 1036),
        [System.Drawing.PointF]::new(640, 1084),
        [System.Drawing.PointF]::new(588, 1036),
        [System.Drawing.PointF]::new(544, 1126)
    ))

    Fill-Linear $g ([System.Drawing.RectangleF]::new(70,410,520,420)) ([System.Drawing.Color]::FromArgb(255, 80, 56, 24)) ([System.Drawing.Color]::FromArgb(255, 208, 152, 62)) 90 { param($brush) $g.FillPath($brush, $wingLeft) }
    Fill-Linear $g ([System.Drawing.RectangleF]::new(690,410,520,420)) ([System.Drawing.Color]::FromArgb(255, 80, 56, 24)) ([System.Drawing.Color]::FromArgb(255, 208, 152, 62)) 90 { param($brush) $g.FillPath($brush, $wingRight) }
    Fill-Linear $g ([System.Drawing.RectangleF]::new(410,80,460,1090)) ([System.Drawing.Color]::FromArgb(255, 72, 74, 82)) ([System.Drawing.Color]::FromArgb(255, 222, 176, 74)) 90 { param($brush) $g.FillPath($brush, $body) }
    Fill-Linear $g ([System.Drawing.RectangleF]::new(544,42,192,202)) ([System.Drawing.Color]::FromArgb(255, 255, 222, 118)) ([System.Drawing.Color]::FromArgb(255, 172, 126, 28)) 90 { param($brush) $g.FillPath($brush, $crown) }

    $outline = New-Pen ([System.Drawing.Color]::FromArgb(238, 255, 236, 198)) 12
    $g.DrawPath($outline, $wingLeft)
    $g.DrawPath($outline, $wingRight)
    $g.DrawPath($outline, $body)
    $g.DrawPath($outline, $crown)
    $outline.Dispose()

    Draw-HexPanel $g 574 866 132 192 ([System.Drawing.Color]::FromArgb(255, 255, 246, 222)) ([System.Drawing.Color]::FromArgb(255, 255, 180, 78)) ([System.Drawing.Color]::FromArgb(220, 255, 236, 208))
    Draw-HexPanel $g 548 582 186 228 ([System.Drawing.Color]::FromArgb(255, 224, 196, 120)) ([System.Drawing.Color]::FromArgb(255, 120, 102, 72)) ([System.Drawing.Color]::FromArgb(176, 250, 224, 178))
    Draw-HexPanel $g 480 340 320 198 ([System.Drawing.Color]::FromArgb(255, 194, 160, 102)) ([System.Drawing.Color]::FromArgb(255, 98, 82, 58)) ([System.Drawing.Color]::FromArgb(172, 244, 218, 170))

    $gridPen = New-Pen ([System.Drawing.Color]::FromArgb(158, 255, 246, 214)) 7
    $g.DrawLine($gridPen, 640, 1110, 640, 248)
    $g.DrawLine($gridPen, 290, 652, 530, 606)
    $g.DrawLine($gridPen, 990, 652, 750, 606)
    $g.DrawLine($gridPen, 218, 470, 488, 546)
    $g.DrawLine($gridPen, 1062, 470, 792, 546)
    $gridPen.Dispose()

    Draw-EngineGlow $g 520 1016 84 118 ([System.Drawing.Color]::FromArgb(255, 255, 226, 160)) ([System.Drawing.Color]::FromArgb(255, 255, 116, 30))
    Draw-EngineGlow $g 598 988 86 110 ([System.Drawing.Color]::FromArgb(255, 255, 226, 160)) ([System.Drawing.Color]::FromArgb(255, 255, 116, 30))
    Draw-EngineGlow $g 678 1016 84 118 ([System.Drawing.Color]::FromArgb(255, 255, 226, 160)) ([System.Drawing.Color]::FromArgb(255, 255, 116, 30))

    $wingLeft.Dispose()
    $wingRight.Dispose()
    $body.Dispose()
    $crown.Dispose()

    Save-Bitmap $ctx $path
}

function Draw-Bullet($path, $edgeColor, $coreColor, $stripeColor) {
    $ctx = New-Bitmap 192 320
    $g = $ctx.Graphics
    $g.Clear([System.Drawing.Color]::Transparent)

    $body = New-Path ([System.Drawing.PointF[]]@(
        [System.Drawing.PointF]::new(96, 16),
        [System.Drawing.PointF]::new(138, 74),
        [System.Drawing.PointF]::new(128, 272),
        [System.Drawing.PointF]::new(96, 306),
        [System.Drawing.PointF]::new(64, 272),
        [System.Drawing.PointF]::new(54, 74)
    ))
    Fill-Linear $g ([System.Drawing.RectangleF]::new(52,16,88,290)) $edgeColor $coreColor 90 { param($brush) $g.FillPath($brush, $body) }
    $outline = New-Pen ([System.Drawing.Color]::FromArgb(220, 255, 250, 240)) 4
    $g.DrawPath($outline, $body)
    $outline.Dispose()

    $stripePen = New-Pen $stripeColor 5
    $g.DrawLine($stripePen, 96, 44, 96, 264)
    $stripePen.Dispose()

    $shine = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(160,255,255,255))
    $g.FillEllipse($shine, 78, 50, 20, 88)
    $shine.Dispose()
    $body.Dispose()

    Save-Bitmap $ctx $path
}

function Draw-Flash($path) {
    $ctx = New-Bitmap 256 256
    $g = $ctx.Graphics
    $g.Clear([System.Drawing.Color]::Transparent)

    $pts = [System.Drawing.PointF[]]@(
        [System.Drawing.PointF]::new(128, 10),
        [System.Drawing.PointF]::new(162, 76),
        [System.Drawing.PointF]::new(246, 128),
        [System.Drawing.PointF]::new(162, 180),
        [System.Drawing.PointF]::new(128, 246),
        [System.Drawing.PointF]::new(94, 180),
        [System.Drawing.PointF]::new(10, 128),
        [System.Drawing.PointF]::new(94, 76)
    )
    $pathObj = New-Path $pts
    Fill-PathGradient $g $pathObj ([System.Drawing.Color]::FromArgb(255,255,250,230)) ([System.Drawing.Color]::FromArgb(0,255,166,48))
    $coreBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(132,255,255,255))
    $g.FillEllipse($coreBrush, 96, 96, 64, 64)
    $coreBrush.Dispose()
    $pathObj.Dispose()

    Save-Bitmap $ctx $path
}

function Draw-SkillNovaIcon([string]$path) {
    $ctx = New-Bitmap 256 256
    $g = $ctx.Graphics
    $g.Clear([System.Drawing.Color]::Transparent)

    $outer = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(44, 64, 198, 236))
    $g.FillEllipse($outer, 18, 18, 220, 220)
    $outer.Dispose()

    $ringPath = New-Object System.Drawing.Drawing2D.GraphicsPath
    $ringPath.AddEllipse(28, 28, 200, 200)
    Fill-PathGradient $g $ringPath ([System.Drawing.Color]::FromArgb(255, 224, 248, 255)) ([System.Drawing.Color]::FromArgb(0, 78, 208, 255))
    $ringPath.Dispose()

    $coreBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(220, 238, 252, 255))
    $g.FillEllipse($coreBrush, 82, 82, 92, 92)
    $coreBrush.Dispose()

    $pen = New-Pen ([System.Drawing.Color]::FromArgb(220, 208, 246, 255)) 8
    for ($i = 0; $i -lt 8; $i++) {
        $angle = [Math]::PI * 2 * $i / 8
        $x1 = 128 + [Math]::Cos($angle) * 22
        $y1 = 128 + [Math]::Sin($angle) * 22
        $x2 = 128 + [Math]::Cos($angle) * 86
        $y2 = 128 + [Math]::Sin($angle) * 86
        $g.DrawLine($pen, $x1, $y1, $x2, $y2)
    }
    $pen.Dispose()

    Save-Bitmap $ctx $path
}

function Draw-SkillOverdriveIcon([string]$path) {
    $ctx = New-Bitmap 256 256
    $g = $ctx.Graphics
    $g.Clear([System.Drawing.Color]::Transparent)

    $bg = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(38, 255, 152, 48))
    $g.FillEllipse($bg, 18, 18, 220, 220)
    $bg.Dispose()

    $chevronA = New-Path ([System.Drawing.PointF[]]@(
        [System.Drawing.PointF]::new(128, 28),
        [System.Drawing.PointF]::new(184, 118),
        [System.Drawing.PointF]::new(152, 118),
        [System.Drawing.PointF]::new(210, 226),
        [System.Drawing.PointF]::new(114, 152),
        [System.Drawing.PointF]::new(144, 152),
        [System.Drawing.PointF]::new(78, 28)
    ))
    Fill-PathGradient $g $chevronA ([System.Drawing.Color]::FromArgb(255, 255, 242, 214)) ([System.Drawing.Color]::FromArgb(0, 255, 136, 40))
    $chevronA.Dispose()

    $chevronB = New-Path ([System.Drawing.PointF[]]@(
        [System.Drawing.PointF]::new(128, 52),
        [System.Drawing.PointF]::new(166, 118),
        [System.Drawing.PointF]::new(140, 118),
        [System.Drawing.PointF]::new(182, 194),
        [System.Drawing.PointF]::new(110, 138),
        [System.Drawing.PointF]::new(130, 138),
        [System.Drawing.PointF]::new(88, 52)
    ))
    Fill-PathGradient $g $chevronB ([System.Drawing.Color]::FromArgb(220, 255, 255, 255)) ([System.Drawing.Color]::FromArgb(0, 255, 198, 72))
    $chevronB.Dispose()

    $outline = New-Pen ([System.Drawing.Color]::FromArgb(220, 255, 236, 198)) 6
    $g.DrawEllipse($outline, 30, 30, 196, 196)
    $outline.Dispose()

    Save-Bitmap $ctx $path
}

function Draw-SkillNovaCast([string]$path) {
    $ctx = New-Bitmap 512 512
    $g = $ctx.Graphics
    $g.Clear([System.Drawing.Color]::Transparent)

    $ring1 = New-Object System.Drawing.Drawing2D.GraphicsPath
    $ring1.AddEllipse(46, 46, 420, 420)
    Fill-PathGradient $g $ring1 ([System.Drawing.Color]::FromArgb(210, 228, 250, 255)) ([System.Drawing.Color]::FromArgb(0, 86, 210, 255))
    $ring1.Dispose()

    $ring2 = New-Object System.Drawing.Drawing2D.GraphicsPath
    $ring2.AddEllipse(118, 118, 276, 276)
    Fill-PathGradient $g $ring2 ([System.Drawing.Color]::FromArgb(92, 214, 246, 255)) ([System.Drawing.Color]::FromArgb(0, 214, 246, 255))
    $ring2.Dispose()

    $coreBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(162, 248, 255, 255))
    $g.FillEllipse($coreBrush, 182, 182, 148, 148)
    $coreBrush.Dispose()

    $pen = New-Pen ([System.Drawing.Color]::FromArgb(218, 210, 246, 255)) 12
    for ($i = 0; $i -lt 12; $i++) {
        $angle = [Math]::PI * 2 * $i / 12
        $x1 = 256 + [Math]::Cos($angle) * 56
        $y1 = 256 + [Math]::Sin($angle) * 56
        $x2 = 256 + [Math]::Cos($angle) * 186
        $y2 = 256 + [Math]::Sin($angle) * 186
        $g.DrawLine($pen, $x1, $y1, $x2, $y2)
    }
    $pen.Dispose()

    Save-Bitmap $ctx $path
}

function Draw-SkillOverdriveCast([string]$path) {
    $ctx = New-Bitmap 512 512
    $g = $ctx.Graphics
    $g.Clear([System.Drawing.Color]::Transparent)

    $flare = New-Path ([System.Drawing.PointF[]]@(
        [System.Drawing.PointF]::new(256, 20),
        [System.Drawing.PointF]::new(344, 126),
        [System.Drawing.PointF]::new(304, 126),
        [System.Drawing.PointF]::new(418, 312),
        [System.Drawing.PointF]::new(286, 256),
        [System.Drawing.PointF]::new(286, 492),
        [System.Drawing.PointF]::new(226, 492),
        [System.Drawing.PointF]::new(226, 256),
        [System.Drawing.PointF]::new(94, 312),
        [System.Drawing.PointF]::new(208, 126),
        [System.Drawing.PointF]::new(168, 126)
    ))
    Fill-PathGradient $g $flare ([System.Drawing.Color]::FromArgb(255, 255, 242, 210)) ([System.Drawing.Color]::FromArgb(0, 255, 146, 52))
    $flare.Dispose()

    $core = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(108, 255, 236, 182))
    $g.FillEllipse($core, 146, 134, 220, 220)
    $core.Dispose()

    $outline = New-Pen ([System.Drawing.Color]::FromArgb(220, 255, 232, 188)) 10
    $g.DrawEllipse($outline, 112, 98, 288, 288)
    $outline.Dispose()

    Save-Bitmap $ctx $path
}

function Draw-DamagedVariant([string]$sourcePath, [string]$targetPath, [System.Drawing.Color]$accentColor) {
    $img = [System.Drawing.Bitmap]::FromFile($sourcePath)
    $ctx = New-Bitmap $img.Width $img.Height
    $g = $ctx.Graphics
    $g.DrawImage($img, 0, 0, $img.Width, $img.Height)

    $smokeBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(120, 28, 20, 18))
    $g.FillEllipse($smokeBrush, $img.Width * 0.26, $img.Height * 0.28, $img.Width * 0.18, $img.Height * 0.16)
    $g.FillEllipse($smokeBrush, $img.Width * 0.54, $img.Height * 0.52, $img.Width * 0.2, $img.Height * 0.18)
    $smokeBrush.Dispose()

    $burnPen = New-Pen([System.Drawing.Color]::FromArgb(180, 34, 18, 18), 8)
    $g.DrawLine($burnPen, $img.Width * 0.36, $img.Height * 0.34, $img.Width * 0.56, $img.Height * 0.58)
    $g.DrawLine($burnPen, $img.Width * 0.58, $img.Height * 0.22, $img.Width * 0.46, $img.Height * 0.42)
    $burnPen.Dispose()

    $sparkPen = New-Pen($accentColor, 4)
    $g.DrawLine($sparkPen, $img.Width * 0.3, $img.Height * 0.36, $img.Width * 0.38, $img.Height * 0.42)
    $g.DrawLine($sparkPen, $img.Width * 0.57, $img.Height * 0.58, $img.Width * 0.66, $img.Height * 0.64)
    $sparkPen.Dispose()

    $img.Dispose()
    Save-Bitmap $ctx $targetPath
}

function Draw-Fragment {
    param(
        [string]$path,
        [System.Drawing.Color[]]$colors,
        [System.Drawing.Color]$outlineColor,
        [int]$width,
        [int]$height,
        [string]$variant
    )

    $ctx = New-Bitmap $width $height
    $g = $ctx.Graphics
    $g.Clear([System.Drawing.Color]::Transparent)

    if ($variant -eq 'A') {
        $pts = [System.Drawing.PointF[]]@(
            [System.Drawing.PointF]::new($width * 0.18, $height * 0.08),
            [System.Drawing.PointF]::new($width * 0.88, $height * 0.18),
            [System.Drawing.PointF]::new($width * 0.74, $height * 0.92),
            [System.Drawing.PointF]::new($width * 0.08, $height * 0.74)
        )
    } else {
        $pts = [System.Drawing.PointF[]]@(
            [System.Drawing.PointF]::new($width * 0.42, $height * 0.04),
            [System.Drawing.PointF]::new($width * 0.94, $height * 0.46),
            [System.Drawing.PointF]::new($width * 0.52, $height * 0.96),
            [System.Drawing.PointF]::new($width * 0.06, $height * 0.58)
        )
    }

    $shape = New-Path $pts
    Fill-Linear $g ([System.Drawing.RectangleF]::new(0,0,$width,$height)) $colors[0] $colors[1] 90 { param($brush) $g.FillPath($brush, $shape) }
    $outline = New-Pen $outlineColor 5
    $g.DrawPath($outline, $shape)
    $outline.Dispose()

    $linePen = New-Pen ([System.Drawing.Color]::FromArgb(120,255,255,255)) 3
    $g.DrawLine($linePen, $width * 0.24, $height * 0.24, $width * 0.7, $height * 0.72)
    $linePen.Dispose()

    $shape.Dispose()
    Save-Bitmap $ctx $path
}

Draw-PlayerShip (Join-Path $root 'PlayerShip.png')
Draw-PlayerShipVariant (Join-Path $root 'PlayerShip_Balanced.png') 'Balanced'
Draw-PlayerShipVariant (Join-Path $root 'PlayerShip_Rapid.png') 'Rapid'
Draw-PlayerShipVariant (Join-Path $root 'PlayerShip_Heavy.png') 'Heavy'
Draw-EnemyShip (Join-Path $root 'EnemyShip.png')
Draw-BossShip (Join-Path $root 'BossShip.png')
Draw-Bullet (Join-Path $root 'PlayerBullet.png') ([System.Drawing.Color]::FromArgb(255, 64, 176, 214)) ([System.Drawing.Color]::FromArgb(255, 222, 244, 250)) ([System.Drawing.Color]::FromArgb(200, 255, 255, 255))
Draw-Bullet (Join-Path $root 'EnemyBullet.png') ([System.Drawing.Color]::FromArgb(255, 156, 58, 28)) ([System.Drawing.Color]::FromArgb(255, 255, 226, 166)) ([System.Drawing.Color]::FromArgb(200, 255, 245, 220))
Draw-Flash (Join-Path $root 'MuzzleFlash.png')
Draw-SkillNovaIcon (Join-Path $root 'SkillNovaIcon.png')
Draw-SkillOverdriveIcon (Join-Path $root 'SkillOverdriveIcon.png')
Draw-SkillNovaCast (Join-Path $root 'SkillNovaCast.png')
Draw-SkillOverdriveCast (Join-Path $root 'SkillOverdriveCast.png')
Draw-DamagedVariant (Join-Path $root 'PlayerShip.png') (Join-Path $root 'PlayerShip_Damaged.png') ([System.Drawing.Color]::FromArgb(210, 110, 225, 255))
Draw-DamagedVariant (Join-Path $root 'PlayerShip_Balanced.png') (Join-Path $root 'PlayerShip_Balanced_Damaged.png') ([System.Drawing.Color]::FromArgb(210, 176, 236, 255))
Draw-DamagedVariant (Join-Path $root 'PlayerShip_Rapid.png') (Join-Path $root 'PlayerShip_Rapid_Damaged.png') ([System.Drawing.Color]::FromArgb(210, 255, 208, 146))
Draw-DamagedVariant (Join-Path $root 'PlayerShip_Heavy.png') (Join-Path $root 'PlayerShip_Heavy_Damaged.png') ([System.Drawing.Color]::FromArgb(210, 255, 188, 128))
Draw-DamagedVariant (Join-Path $root 'EnemyShip.png') (Join-Path $root 'EnemyShip_Damaged.png') ([System.Drawing.Color]::FromArgb(210, 255, 168, 120))
Draw-DamagedVariant (Join-Path $root 'BossShip.png') (Join-Path $root 'BossShip_Damaged.png') ([System.Drawing.Color]::FromArgb(210, 255, 198, 132))
Draw-Fragment -path (Join-Path $root 'PlayerFragmentA.png') -colors @([System.Drawing.Color]::FromArgb(255, 68, 108, 138), [System.Drawing.Color]::FromArgb(255, 198, 214, 226)) -outlineColor ([System.Drawing.Color]::FromArgb(220, 230, 240, 245)) -width 280 -height 220 -variant 'A'
Draw-Fragment -path (Join-Path $root 'PlayerFragmentB.png') -colors @([System.Drawing.Color]::FromArgb(255, 42, 84, 112), [System.Drawing.Color]::FromArgb(255, 132, 186, 214)) -outlineColor ([System.Drawing.Color]::FromArgb(220, 230, 240, 245)) -width 280 -height 220 -variant 'B'
Draw-Fragment -path (Join-Path $root 'PlayerBalancedFragmentA.png') -colors @([System.Drawing.Color]::FromArgb(255, 58, 64, 72), [System.Drawing.Color]::FromArgb(255, 164, 164, 156)) -outlineColor ([System.Drawing.Color]::FromArgb(220, 236, 224, 206)) -width 280 -height 220 -variant 'A'
Draw-Fragment -path (Join-Path $root 'PlayerBalancedFragmentB.png') -colors @([System.Drawing.Color]::FromArgb(255, 48, 54, 62), [System.Drawing.Color]::FromArgb(255, 118, 150, 164)) -outlineColor ([System.Drawing.Color]::FromArgb(220, 226, 216, 202)) -width 280 -height 220 -variant 'B'
Draw-Fragment -path (Join-Path $root 'PlayerRapidFragmentA.png') -colors @([System.Drawing.Color]::FromArgb(255, 42, 42, 48), [System.Drawing.Color]::FromArgb(255, 176, 148, 104)) -outlineColor ([System.Drawing.Color]::FromArgb(220, 236, 214, 194)) -width 280 -height 220 -variant 'A'
Draw-Fragment -path (Join-Path $root 'PlayerRapidFragmentB.png') -colors @([System.Drawing.Color]::FromArgb(255, 36, 36, 42), [System.Drawing.Color]::FromArgb(255, 118, 152, 188)) -outlineColor ([System.Drawing.Color]::FromArgb(220, 226, 210, 192)) -width 280 -height 220 -variant 'B'
Draw-Fragment -path (Join-Path $root 'PlayerHeavyFragmentA.png') -colors @([System.Drawing.Color]::FromArgb(255, 58, 48, 38), [System.Drawing.Color]::FromArgb(255, 182, 144, 88)) -outlineColor ([System.Drawing.Color]::FromArgb(220, 240, 216, 188)) -width 280 -height 220 -variant 'A'
Draw-Fragment -path (Join-Path $root 'PlayerHeavyFragmentB.png') -colors @([System.Drawing.Color]::FromArgb(255, 50, 42, 34), [System.Drawing.Color]::FromArgb(255, 228, 138, 62)) -outlineColor ([System.Drawing.Color]::FromArgb(220, 236, 210, 184)) -width 280 -height 220 -variant 'B'
Draw-Fragment -path (Join-Path $root 'EnemyFragmentA.png') -colors @([System.Drawing.Color]::FromArgb(255, 42, 30, 34), [System.Drawing.Color]::FromArgb(255, 164, 70, 62)) -outlineColor ([System.Drawing.Color]::FromArgb(220, 244, 210, 202)) -width 280 -height 220 -variant 'A'
Draw-Fragment -path (Join-Path $root 'EnemyFragmentB.png') -colors @([System.Drawing.Color]::FromArgb(255, 50, 34, 38), [System.Drawing.Color]::FromArgb(255, 132, 54, 48)) -outlineColor ([System.Drawing.Color]::FromArgb(220, 244, 210, 202)) -width 280 -height 220 -variant 'B'
Draw-Fragment -path (Join-Path $root 'BossFragmentA.png') -colors @([System.Drawing.Color]::FromArgb(255, 84, 70, 44), [System.Drawing.Color]::FromArgb(255, 218, 176, 92)) -outlineColor ([System.Drawing.Color]::FromArgb(220, 255, 234, 198)) -width 360 -height 280 -variant 'A'
Draw-Fragment -path (Join-Path $root 'BossFragmentB.png') -colors @([System.Drawing.Color]::FromArgb(255, 76, 64, 38), [System.Drawing.Color]::FromArgb(255, 178, 136, 64)) -outlineColor ([System.Drawing.Color]::FromArgb(220, 255, 234, 198)) -width 360 -height 280 -variant 'B'

function Draw-EliteVariant([string]$sourcePath, [string]$targetPath) {
    $img = [System.Drawing.Bitmap]::FromFile($sourcePath)
    $ctx = New-Bitmap $img.Width $img.Height
    $g = $ctx.Graphics
    $g.DrawImage($img, 0, 0, $img.Width, $img.Height)

    $violet = [System.Drawing.Color]::FromArgb(210, 122, 210, 255)
    $cyan = [System.Drawing.Color]::FromArgb(210, 180, 255, 255)
    Draw-HexPanel $g 430 240 164 132 $cyan $violet ([System.Drawing.Color]::FromArgb(220, 236, 248, 255))
    Draw-HexPanel $g 280 394 122 176 ([System.Drawing.Color]::FromArgb(255, 62, 70, 98)) ([System.Drawing.Color]::FromArgb(255, 178, 114, 210)) ([System.Drawing.Color]::FromArgb(220, 228, 210, 255))
    Draw-HexPanel $g 622 394 122 176 ([System.Drawing.Color]::FromArgb(255, 62, 70, 98)) ([System.Drawing.Color]::FromArgb(255, 178, 114, 210)) ([System.Drawing.Color]::FromArgb(220, 228, 210, 255))

    $pen = New-Pen ([System.Drawing.Color]::FromArgb(190, 212, 244, 255)) 7
    $g.DrawLine($pen, 236, 428, 430, 404)
    $g.DrawLine($pen, 788, 428, 594, 404)
    $g.DrawLine($pen, 300, 700, 430, 606)
    $g.DrawLine($pen, 724, 700, 594, 606)
    $pen.Dispose()

    Draw-EngineGlow $g 422 802 72 116 ([System.Drawing.Color]::FromArgb(255, 222, 248, 255)) ([System.Drawing.Color]::FromArgb(255, 122, 204, 255))
    Draw-EngineGlow $g 530 802 72 116 ([System.Drawing.Color]::FromArgb(255, 222, 248, 255)) ([System.Drawing.Color]::FromArgb(255, 122, 204, 255))

    $img.Dispose()
    Save-Bitmap $ctx $targetPath
}

function Draw-BossPhase2Variant([string]$sourcePath, [string]$targetPath) {
    $img = [System.Drawing.Bitmap]::FromFile($sourcePath)
    $ctx = New-Bitmap $img.Width $img.Height
    $g = $ctx.Graphics
    $g.DrawImage($img, 0, 0, $img.Width, $img.Height)

    $glow = [System.Drawing.Color]::FromArgb(255, 120, 232, 255)
    $core = [System.Drawing.Color]::FromArgb(255, 235, 250, 255)
    Draw-HexPanel $g 548 234 184 138 $core $glow ([System.Drawing.Color]::FromArgb(220, 238, 248, 255))
    Draw-HexPanel $g 470 430 340 200 ([System.Drawing.Color]::FromArgb(255, 88, 92, 110)) ([System.Drawing.Color]::FromArgb(255, 142, 214, 255)) ([System.Drawing.Color]::FromArgb(220, 228, 246, 255))
    Draw-HexPanel $g 516 700 248 222 ([System.Drawing.Color]::FromArgb(255, 92, 98, 124)) ([System.Drawing.Color]::FromArgb(255, 162, 228, 255)) ([System.Drawing.Color]::FromArgb(220, 228, 246, 255))

    $pen = New-Pen ([System.Drawing.Color]::FromArgb(180, 214, 242, 255)) 9
    $g.DrawLine($pen, 202, 466, 488, 538)
    $g.DrawLine($pen, 1078, 466, 792, 538)
    $g.DrawLine($pen, 306, 780, 552, 684)
    $g.DrawLine($pen, 974, 780, 728, 684)
    $pen.Dispose()

    Draw-EngineGlow $g 502 1002 92 128 ([System.Drawing.Color]::FromArgb(255, 232, 246, 255)) ([System.Drawing.Color]::FromArgb(255, 114, 210, 255))
    Draw-EngineGlow $g 592 976 96 118 ([System.Drawing.Color]::FromArgb(255, 232, 246, 255)) ([System.Drawing.Color]::FromArgb(255, 114, 210, 255))
    Draw-EngineGlow $g 690 1002 92 128 ([System.Drawing.Color]::FromArgb(255, 232, 246, 255)) ([System.Drawing.Color]::FromArgb(255, 114, 210, 255))

    $img.Dispose()
    Save-Bitmap $ctx $targetPath
}

Draw-EliteVariant (Join-Path $root 'EnemyShip.png') (Join-Path $root 'EnemyShip_Elite.png')
Draw-DamagedVariant (Join-Path $root 'EnemyShip_Elite.png') (Join-Path $root 'EnemyShip_Elite_Damaged.png') ([System.Drawing.Color]::FromArgb(210, 188, 228, 255))
Draw-Fragment -path (Join-Path $root 'EnemyEliteFragmentA.png') -colors @([System.Drawing.Color]::FromArgb(255, 58, 60, 88), [System.Drawing.Color]::FromArgb(255, 186, 126, 224)) -outlineColor ([System.Drawing.Color]::FromArgb(220, 230, 218, 255)) -width 280 -height 220 -variant 'A'
Draw-Fragment -path (Join-Path $root 'EnemyEliteFragmentB.png') -colors @([System.Drawing.Color]::FromArgb(255, 64, 66, 92), [System.Drawing.Color]::FromArgb(255, 144, 206, 255)) -outlineColor ([System.Drawing.Color]::FromArgb(220, 230, 218, 255)) -width 280 -height 220 -variant 'B'
Draw-BossPhase2Variant (Join-Path $root 'BossShip.png') (Join-Path $root 'BossShip_Phase2.png')
Draw-DamagedVariant (Join-Path $root 'BossShip_Phase2.png') (Join-Path $root 'BossShip_Phase2_Damaged.png') ([System.Drawing.Color]::FromArgb(210, 176, 232, 255))
Draw-Fragment -path (Join-Path $root 'BossPhase2FragmentA.png') -colors @([System.Drawing.Color]::FromArgb(255, 88, 92, 120), [System.Drawing.Color]::FromArgb(255, 162, 216, 255)) -outlineColor ([System.Drawing.Color]::FromArgb(220, 228, 240, 255)) -width 360 -height 280 -variant 'A'
Draw-Fragment -path (Join-Path $root 'BossPhase2FragmentB.png') -colors @([System.Drawing.Color]::FromArgb(255, 82, 86, 114), [System.Drawing.Color]::FromArgb(255, 128, 198, 255)) -outlineColor ([System.Drawing.Color]::FromArgb(220, 228, 240, 255)) -width 360 -height 280 -variant 'B'
