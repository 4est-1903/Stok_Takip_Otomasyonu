using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;
using iTextSharp.text;  
using iTextSharp.text.pdf;

namespace StokTakipOtomasyonu
{

    public partial class frmSatisSayfa : Form
    {
        public frmSatisSayfa()
        {
            InitializeComponent();
        }

        MySqlConnection baglan = new MySqlConnection("Server=localhost;Database=stok_takip;Uid=root;Pwd=root;Pooling=Yes");
        DataSet ds = new DataSet();
        bool durum;

        private void frmSatisSayfa_Load(object sender, EventArgs e)
        {
            SepetListele();
            Hesapla();
        }

        private void SepetListele()
        {
            try
            {
                ds.Clear();
                using (MySqlDataAdapter adtr = new MySqlDataAdapter("SELECT * FROM sepet", baglan))
                {
                    adtr.Fill(ds, "sepet");
                    dataGridView1.DataSource = ds.Tables["sepet"];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sepet listeleme hatası: " + ex.Message);
            }
        }

        private void Hesapla()
        {
            try
            {
                baglan.Open();
                MySqlCommand komut = new MySqlCommand("SELECT SUM(toplamfiyat) FROM sepet", baglan);
                object sonuc = komut.ExecuteScalar();
                lblgeneltoplam.Text = sonuc != DBNull.Value ? sonuc.ToString() + " TL" : "0 TL";
            }
            catch (Exception)
            {
                lblgeneltoplam.Text = "0 TL";
            }
            finally
            {
                baglan.Close();
            }
        }

        private void BarkodKontrol()
        {
            durum = true;
            baglan.Open();
            MySqlCommand komut = new MySqlCommand("SELECT * FROM sepet WHERE barkod = @barkod", baglan);
            komut.Parameters.AddWithValue("@barkod", txtbarkod.Text);
            using (MySqlDataReader oku = komut.ExecuteReader())
            {
                if (oku.Read())
                    durum = false;
            }
            baglan.Close();
        }

        private void btnekle_Click(object sender, EventArgs e)
        {
            BarkodKontrol();
            baglan.Open();

            if (durum)
            {
                MySqlCommand komut = new MySqlCommand("INSERT INTO sepet(tc, adsoyad, telefon, barkod, urunad, miktar, satisfiyat, toplamfiyat, tarih) VALUES(@tc, @adsoyad, @telefon, @barkod, @urunad, @miktar, @satisfiyat, @toplamfiyat, @tarih)", baglan);
                komut.Parameters.AddWithValue("@tc", txttckimlik.Text);
                komut.Parameters.AddWithValue("@adsoyad", txtadsoyad.Text);
                komut.Parameters.AddWithValue("@telefon", txttelefon.Text);
                komut.Parameters.AddWithValue("@barkod", txtbarkod.Text);
                komut.Parameters.AddWithValue("@urunad", txturunad.Text);
                komut.Parameters.AddWithValue("@miktar", int.Parse(txturunmiktar.Text));
                komut.Parameters.AddWithValue("@satisfiyat", double.Parse(txtsatisfiyat.Text));
                komut.Parameters.AddWithValue("@toplamfiyat", double.Parse(txttoplamfiyat.Text));
                komut.Parameters.AddWithValue("@tarih", DateTime.Now);
                komut.ExecuteNonQuery();
            }
            else
            {
                MySqlCommand komut2 = new MySqlCommand("UPDATE sepet SET miktar = miktar + @eklenenMiktar, toplamfiyat = (miktar + @eklenenMiktar) * satisfiyat WHERE barkod = @barkod", baglan);
                komut2.Parameters.AddWithValue("@eklenenMiktar", int.Parse(txturunmiktar.Text));
                komut2.Parameters.AddWithValue("@barkod", txtbarkod.Text);
                komut2.ExecuteNonQuery();
            }

            baglan.Close();
            txturunmiktar.Text = "1";
            ds.Tables["sepet"].Clear();
            SepetListele();
            Hesapla();
        }

        private void btnsatisyap_Click(object sender, EventArgs e)
        {
            try
            {
                BaglantiAc();

                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.IsNewRow) continue;

                    string barkod = row.Cells["barkod"].Value.ToString();

                    if (!BarkodMevcutMu(barkod))
                    {
                        MessageBox.Show($"'{barkod}' barkod numarası mevcut değil.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    int mevcutStokMiktar = GetMevcutStokMiktar(barkod);
                    int satisMiktar = int.Parse(row.Cells["miktar"].Value.ToString());

                    if (satisMiktar > mevcutStokMiktar)
                    {
                        MessageBox.Show($"'{row.Cells["urunad"].Value}' ürünü için yeterli stok bulunmamaktadır. Mevcut stok: {mevcutStokMiktar}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Sepetten gelen müşteri bilgilerini kullan
                    string tc = row.Cells["tc"].Value.ToString();
                    string adsoyad = row.Cells["adsoyad"].Value.ToString();
                    string telefon = row.Cells["telefon"].Value.ToString();

                    MySqlCommand komut1 = new MySqlCommand("INSERT INTO satislar(tc, adsoyad, telefon, barkod_no, urun_adi, miktar, birim_fiyati, toplam_fiyat, tarih) VALUES(@tc, @adsoyad, @telefon, @barkod, @urunad, @miktar, @satisfiyat, @toplamfiyat, @tarih)", baglan);
                    komut1.Parameters.AddWithValue("@tc", tc);
                    komut1.Parameters.AddWithValue("@adsoyad", adsoyad);
                    komut1.Parameters.AddWithValue("@telefon", telefon);
                    komut1.Parameters.AddWithValue("@barkod", barkod);
                    komut1.Parameters.AddWithValue("@urunad", row.Cells["urunad"].Value.ToString());
                    komut1.Parameters.AddWithValue("@miktar", satisMiktar);
                    komut1.Parameters.AddWithValue("@satisfiyat", double.Parse(row.Cells["satisfiyat"].Value.ToString()));
                    komut1.Parameters.AddWithValue("@toplamfiyat", double.Parse(row.Cells["toplamfiyat"].Value.ToString()));
                    komut1.Parameters.AddWithValue("@tarih", DateTime.Now);
                    komut1.ExecuteNonQuery();

                    // Stok miktarını güncelle
                    MySqlCommand komut2 = new MySqlCommand("UPDATE urunler SET miktar = miktar - @satisMiktar WHERE barkod_no = @barkod", baglan);
                    komut2.Parameters.AddWithValue("@satisMiktar", satisMiktar);
                    komut2.Parameters.AddWithValue("@barkod", barkod);
                    komut2.ExecuteNonQuery();

                    // Ürünün yeni stok miktarını ve alt limit durumunu kontrol et
                    MySqlCommand komut3 = new MySqlCommand("SELECT miktar, alt_limit FROM urunler WHERE barkod_no = @barkod", baglan);
                    komut3.Parameters.AddWithValue("@barkod", barkod);
                    using (MySqlDataReader oku = komut3.ExecuteReader())
                    {
                        if (oku.Read())
                        {
                            int stokMiktar = Convert.ToInt32(oku["miktar"]);
                            int altLimit = Convert.ToInt32(oku["alt_limit"]);
                            if (stokMiktar < altLimit)
                            {
                                MessageBox.Show($"Dikkat! '{row.Cells["urunad"].Value}' ürünü alt limitin altında!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                }
                pdfYazdir();
                MySqlCommand temizle = new MySqlCommand("DELETE FROM sepet", baglan);
                temizle.ExecuteNonQuery();

                ds.Tables["sepet"].Clear();
                SepetListele();
                Hesapla();

                MessageBox.Show("Satış işlemi tamamlandı.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Satış sırasında hata: " + ex.Message);
            }
            finally
            {
                BaglantiKapat();
            }
        }

        private int GetMevcutStokMiktar(string barkod)
        {
            MySqlCommand komut = new MySqlCommand("SELECT miktar FROM urunler WHERE barkod_no = @barkod", baglan);
            komut.Parameters.AddWithValue("@barkod", barkod);
            object sonuc = komut.ExecuteScalar();
            return sonuc != null ? Convert.ToInt32(sonuc) : 0;
        }

        private void btnsatisiptal_Click(object sender, EventArgs e)
        {
            try
            {
                baglan.Open();
                MySqlCommand temizle = new MySqlCommand("DELETE FROM sepet", baglan);
                temizle.ExecuteNonQuery();
                ds.Tables["sepet"].Clear();
                SepetListele();
                Hesapla();
                MessageBox.Show("Satış iptal edildi.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("İptal sırasında hata: " + ex.Message);
            }
            finally
            {
                baglan.Close();
            }
        }

        private void btnsil_Click(object sender, EventArgs e)
        {
            try
            {
                string barkod = dataGridView1.SelectedRows[0].Cells["barkod"].Value.ToString();
                baglan.Open();
                MySqlCommand komut = new MySqlCommand("DELETE FROM sepet WHERE barkod = @barkod", baglan);
                komut.Parameters.AddWithValue("@barkod", barkod);
                komut.ExecuteNonQuery();
                baglan.Close();

                ds.Tables["sepet"].Clear();
                SepetListele();
                Hesapla();
                MessageBox.Show("Ürün sepetteki kayıttan silindi.", "Silindi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ürün silme sırasında hata: " + ex.Message);
            }
        }

        private bool BarkodMevcutMu(string barkod)
        {
            MySqlCommand komut = new MySqlCommand("SELECT COUNT(*) FROM urunler WHERE barkod_no = @barkod", baglan);
            komut.Parameters.AddWithValue("@barkod", barkod);
            return Convert.ToInt32(komut.ExecuteScalar()) > 0;
        }

        private void btnurunekle_Click(object sender, EventArgs e)
        {
            frmUrunEkle form = new frmUrunEkle();
            form.Show();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            // Gerekli değil
        }

        private void btnmusteriekle_Click(object sender, EventArgs e)
        {
            frmMusteriEkle form = new frmMusteriEkle();
            form.Show();
        }

        private void btnmusterilistele_Click(object sender, EventArgs e)
        {
            frmMusteriListele form = new frmMusteriListele();
            form.Show();
        }

        private void btnurunlistele_Click(object sender, EventArgs e)
        {
            frmUrunListele form = new frmUrunListele();
            form.Show();
        }

        private void btnsatislistele_Click(object sender, EventArgs e)
        {
            frmSatisListele form = new frmSatisListele();
            form.Show();
        }

        private void btnKategori_Click(object sender, EventArgs e)
        {
            frmKategori form = new frmKategori();
            form.Show();
        }

        private void btnMarka_Click(object sender, EventArgs e)
        {
            frmMarka form = new frmMarka();
            form.Show();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void BaglantiAc()
        {
            if (baglan.State == System.Data.ConnectionState.Closed)
            {
                baglan.Open();
            }
        }

        private void BaglantiKapat()
        {
            if (baglan.State == System.Data.ConnectionState.Open)
            {
                baglan.Close();
            }
        }

       


        private void txttckimlik_TextChanged(object sender, EventArgs e)
        {
            baglan.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT adsoyad, telefon FROM musteriler WHERE tc = @tc", baglan);
            cmd.Parameters.AddWithValue("@tc", txttckimlik.Text);
            MySqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                txtadsoyad.Text = dr["adsoyad"].ToString();
                txttelefon.Text = dr["telefon"].ToString();
            }
            else
            {
                txtadsoyad.Text = "";
                txttelefon.Text = "";
            }

            dr.Close();
            baglan.Close();
        }

        private void txtbarkod_TextChanged(object sender, EventArgs e)
        {
            baglan.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT urun_adi, satisfiyat FROM urunler WHERE barkod_no = @barkod", baglan);
            cmd.Parameters.AddWithValue("@barkod", txtbarkod.Text);
            MySqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                txturunad.Text = dr["urun_adi"].ToString();
                txtsatisfiyat.Text = dr["satisfiyat"].ToString();
            }
            else
            {
                txturunad.Text = "";
                txtsatisfiyat.Text = "";
            }

            dr.Close();
            baglan.Close();
        }

        private void txttoplamfiyat_TextChanged(object sender, EventArgs e)
        {

        }

        private void txturunmiktar_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(txturunmiktar.Text) && !string.IsNullOrEmpty(txtsatisfiyat.Text))
                {
                    int miktar = int.Parse(txturunmiktar.Text);
                    double fiyat = double.Parse(txtsatisfiyat.Text);
                    txttoplamfiyat.Text = (miktar * fiyat).ToString("0.00");
                }
                else
                {
                    txttoplamfiyat.Text = "";
                }
            }
            catch
            {
                txttoplamfiyat.Text = "";
            }
        }

        private void brnTemizle_Click(object sender, EventArgs e)
        {
            foreach (Control item in this.Controls)
            {
                if (item is TextBox)
                {
                    item.Text = "";
                }
            }

            foreach (Control group in this.Controls)
            {
                if (group is Panel || group is GroupBox)
                {
                    foreach (Control item in group.Controls)
                    {
                        if (item is TextBox)
                        {
                            item.Text = "";
                        }
                    }
                }
            }

        }

        private void btnYazdir_Click(object sender, EventArgs e)
        {
            pdfYazdir();
        }

        private void pdfYazdir()
        {
            try
            {
                // Kullanıcının dosyayı kaydedeceği yeri seçmesini sağlamak
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "PDF Kaydet";
                saveFileDialog.Filter = "PDF Dosyası|*.pdf";
                saveFileDialog.FileName = $"Satis_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                    return;

                string tamYol = saveFileDialog.FileName;

                // PDF Başlat
                Document doc = new Document(PageSize.A4);
                PdfWriter.GetInstance(doc, new FileStream(tamYol, FileMode.Create));
                doc.Open();

                // Font Tanımlamaları
                Font font = FontFactory.GetFont("Helvetica", 12f); // Başlık için font
                Font smallFont = FontFactory.GetFont("Helvetica", 10f);       // Tablo içeriği için font
                Font totalFont = FontFactory.GetFont("Helvetica", 12f); // Genel toplam için font

                // Başlık
                Paragraph baslik = new Paragraph("SATIS FATURASI", font);
                baslik.Alignment = Element.ALIGN_CENTER;
                doc.Add(baslik);

                doc.Add(new Paragraph("\n"));

                // Müşteri Bilgileri
                string tc = txttckimlik.Text;
                string adSoyad = txtadsoyad.Text;
                string telefon = txttelefon.Text;
                string tarih = DateTime.Now.ToString("dd.MM.yyyy HH:mm");

                doc.Add(new Paragraph($"T.C: {tc}", smallFont));
                doc.Add(new Paragraph($"Ad Soyad: {adSoyad}", smallFont));
                doc.Add(new Paragraph($"Telefon: {telefon}", smallFont));
                doc.Add(new Paragraph($"Tarih: {tarih}", smallFont));
                doc.Add(new Paragraph("\n"));

                // PDF Tablosu
                PdfPTable table = new PdfPTable(4);
                table.WidthPercentage = 100;
                table.AddCell(new PdfPCell(new Phrase("Ürün Adı", smallFont)));
                table.AddCell(new PdfPCell(new Phrase("Miktar", smallFont)));
                table.AddCell(new PdfPCell(new Phrase("Fiyat", smallFont)));
                table.AddCell(new PdfPCell(new Phrase("Toplam", smallFont)));

                // Sepet verilerini çekmek için DataGridView kullanılıyor
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.IsNewRow) continue;

                    string urunAd = row.Cells["urunad"].Value.ToString();
                    string miktar = row.Cells["miktar"].Value.ToString();
                    string fiyat = row.Cells["satisfiyat"].Value.ToString();
                    string toplamFiyat = row.Cells["toplamfiyat"].Value.ToString();

                    table.AddCell(new PdfPCell(new Phrase(urunAd, smallFont)));
                    table.AddCell(new PdfPCell(new Phrase(miktar, smallFont)));
                    table.AddCell(new PdfPCell(new Phrase(fiyat, smallFont)));
                    table.AddCell(new PdfPCell(new Phrase(toplamFiyat, smallFont)));
                }

                // Tabloyu PDF'e Ekleme
                doc.Add(table);

                // Genel Toplam
                doc.Add(new Paragraph("\nToplam Tutar: " + lblgeneltoplam.Text, totalFont));

                // PDF'i Kapat
                doc.Close();

                // Kullanıcıya Bilgi Ver
                MessageBox.Show("PDF başarıyla oluşturuldu:\n" + tamYol, "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("PDF oluşturma hatası: " + ex.Message);
            }
        }
    }
}
