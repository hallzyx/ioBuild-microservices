data "cloudflare_zone" "main" {
  name = var.cloudflare_zone_name
}

resource "cloudflare_record" "demo" {
  zone_id = data.cloudflare_zone.main.id
  name    = var.domain_name
  type    = "A"
  value   = azurerm_public_ip.vm.ip_address
  proxied = true
  ttl     = 1 # 1 = automatic; required to be 1 when proxied
}
