using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace StokTakipOtomasyonu
{
    public partial class frmUrunEkle : Form
    {

        public frmUrunEkle()
        {
            InitializeComponent();
        }

        MySqlConnection baglan = new MySqlConnection("Server=localhost;Database=stok_takip;Uid=root;Pwd=root;");
        bool durum;

        private void frmUrunEkle_Load(object sender, EventArgs e)
        {
            KategoriGetir();
        }

        private void BarkodBilgileriniGetir(string barkod)
        {
            try
            {
                baglan.Open();
                MySqlCommand komut = new MySqlCommand("SELECT * FROM urunler WHERE barkod_no LIKE @barkod ORDER BY barkod_no ASC LIMIT 1", baglan);
                komut.Parameters.AddWithValue("@barkod", barkod + "%");  // Başlangıçtaki girilen barkodu alıyoruz

                MySqlDataReader oku = komut.ExecuteReader();

                if (oku.HasRows)
                {
                    oku.Read();
                    txtMevcutKategori.Text = oku["kategori"].ToString();
                    txtMevcutMarka.Text = oku["marka"].ToString();
                    txtMevcutUrunAdi.Text = oku["urun_adi"].ToString();
                    txtMevcutMiktar.Text = oku["miktar"].ToString();
                    txtMevcutAlis.Text = oku["alisfiyat"].ToString();
                    txtMevcutSatis.Text = oku["satisfiyat"].ToString();
                    txtMevcutAltLimit.Text = oku["alt_limit"].ToString();
                }
                else
                {
                    MessageBox.Show("Bu barkod numarasına ait ürün bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                oku.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veri getirilirken bir hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                baglan.Close();
            }
        }

        private void KategoriGetir()
        {
            comboYeniKategori.Items.Clear();
            baglan.Open();
            MySqlCommand komut = new MySqlCommand("SELECT * FROM kategoriler", baglan);
            MySqlDataReader oku = komut.ExecuteReader();
            while (oku.Read())
            {
                comboYeniKategori.Items.Add(oku["kategori_adi"].ToString());
            }
            oku.Close();
            baglan.Close();
        }

        private void comboYeniKategori_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboYeniMarka.Items.Clear();
            baglan.Open();
            MySqlCommand komut = new MySqlCommand("SELECT marka_adi FROM markalar m INNER JOIN kategoriler k ON m.kategori_id = k.id WHERE k.kategori_adi = @kategori", baglan);
            komut.Parameters.AddWithValue("@kategori", comboYeniKategori.Text);
            MySqlDataReader oku = komut.ExecuteReader();
            while (oku.Read())
            {
                comboYeniMarka.Items.Add(oku["marka_adi"].ToString());
            }
            oku.Close();
            baglan.Close();
        }

        private void BarkodKontrol()
        {
            durum = true;
            if (string.IsNullOrWhiteSpace(txtYeniBarkod.Text)) return;

            try
            {
                baglan.Open();

                MySqlCommand komut = new MySqlCommand("SELECT barkod_no FROM urunler WHERE barkod_no = @barkod", baglan);
                komut.Parameters.AddWithValue("@barkod", txtYeniBarkod.Text);
                object sonuc = komut.ExecuteScalar();

                if (sonuc != null)
                {
                    durum = false; 
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Barkod kontrolü sırasında hata oluştu: " + ex.Message);
            }
            finally
            {
                baglan.Close(); 
            }
        }

        private void btnYeniEkle_Click(object sender, EventArgs e)
        {
            BarkodKontrol();
            if (durum)
            {
                try
                {
                    baglan.Open();
                    MySqlCommand komut = new MySqlCommand(
                        "INSERT INTO urunler (barkod_no, kategori, marka, urun_adi, miktar, alisfiyat, satisfiyat, tarih, alt_limit) " +
                        "VALUES (@barkod, @kategori, @marka, @urunad, @miktar, @alisfiyat, @satisfiyat, @tarih, @altlimit)", baglan);
                    komut.Parameters.AddWithValue("@barkod", txtYeniBarkod.Text.Trim());
                    komut.Parameters.AddWithValue("@kategori", comboYeniKategori.Text);
                    komut.Parameters.AddWithValue("@marka", comboYeniMarka.Text);
                    komut.Parameters.AddWithValue("@urunad", txtYeniUrunAdi.Text.Trim());
                    komut.Parameters.AddWithValue("@miktar", int.Parse(txtYeniMiktar.Text));
                    komut.Parameters.AddWithValue("@alisfiyat", double.Parse(txtYeniAlis.Text));
                    komut.Parameters.AddWithValue("@satisfiyat", double.Parse(txtYeniSatis.Text));
                    komut.Parameters.AddWithValue("@tarih", DateTime.Now.ToString("yyyy-MM-dd"));
                    komut.Parameters.AddWithValue("@altlimit", string.IsNullOrWhiteSpace(txtAltLimitYeni.Text) ? 0 : int.Parse(txtAltLimitYeni.Text));
                    komut.ExecuteNonQuery();
                    baglan.Close();
                    MessageBox.Show("Ürün başarıyla eklendi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Temizle();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ürün eklenirken hata oluştu: " + ex.Message);
                    baglan.Close();
                }
            }
            else
            {
                MessageBox.Show("Bu barkod zaten kayıtlı veya geçersiz giriş yaptınız!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Temizle()
        {
            foreach (Control item in groupBox1.Controls)
            {
                if (item is TextBox || item is ComboBox)
                    item.Text = "";
            }
            comboYeniMarka.Items.Clear();
        }

        private void btnYeniEkle_Click_1(object sender, EventArgs e)
        {
            BarkodKontrol();
            if (durum)  
            {
                try
                {
                    baglan.Open();
                    MySqlCommand komut = new MySqlCommand(
                        "INSERT INTO urunler (barkod_no, kategori, marka, urun_adi, miktar, alisfiyat, satisfiyat, tarih, alt_limit) " +
                        "VALUES (@barkod, @kategori, @marka, @urunad, @miktar, @alisfiyat, @satisfiyat, @tarih, @altlimit)", baglan);
                    komut.Parameters.AddWithValue("@barkod", txtYeniBarkod.Text.Trim());
                    komut.Parameters.AddWithValue("@kategori", comboYeniKategori.Text);
                    komut.Parameters.AddWithValue("@marka", comboYeniMarka.Text);
                    komut.Parameters.AddWithValue("@urunad", txtYeniUrunAdi.Text.Trim());
                    komut.Parameters.AddWithValue("@miktar", int.Parse(txtYeniMiktar.Text));
                    komut.Parameters.AddWithValue("@alisfiyat", double.Parse(txtYeniAlis.Text));
                    komut.Parameters.AddWithValue("@satisfiyat", double.Parse(txtYeniSatis.Text));
                    komut.Parameters.AddWithValue("@tarih", DateTime.Now.ToString("yyyy-MM-dd"));
                    komut.Parameters.AddWithValue("@altlimit", string.IsNullOrWhiteSpace(txtAltLimitYeni.Text) ? 0 : int.Parse(txtAltLimitYeni.Text));
                    komut.ExecuteNonQuery();
                    baglan.Close();
                    MessageBox.Show("Ürün başarıyla eklendi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Temizle();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ürün eklenirken hata oluştu: " + ex.Message);
                    baglan.Close();
                }
            }
            else
            {
                MessageBox.Show("Bu barkod zaten kayıtlı veya geçersiz giriş yaptınız!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnMevcutEkle_Click(object sender, EventArgs e)
        {
			// Barkod numarasını almak
			string barkodNo = txtMevcutBarkod.Text.Trim(); // Bu kısmı, kullanıcının barkod numarasını girdiği TextBox'a göre değiştirin.

			// Girilen miktarı almak
			int eklenenMiktar = int.Parse(txtMevcutMiktar.Text.Trim()); // Bu kısmı da, kullanıcıdan eklenen miktarı alacağınız TextBox'a göre değiştirin.

			// Eğer miktar 0'dan küçükse, uyarı verelim
			if (eklenenMiktar <= 0)
			{
				MessageBox.Show("Eklemek istediğiniz miktar geçerli olmalıdır.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			try
			{
				// Mevcut ürünü veritabanında sorguluyoruz
				baglan.Open();
				MySqlCommand komut = new MySqlCommand("SELECT miktar FROM urunler WHERE barkod_no = @barkod", baglan);
				komut.Parameters.AddWithValue("@barkod", barkodNo);
				object sonuc = komut.ExecuteScalar();

				// Eğer ürün bulunmuşsa, mevcut miktarı arttırıyoruz
				if (sonuc != null)
				{
					int mevcutMiktar = Convert.ToInt32(sonuc);

					// Yeni miktarı hesaplıyoruz
					int yeniMiktar = mevcutMiktar + eklenenMiktar;

					// Ürün miktarını güncelliyoruz
					MySqlCommand guncelleKomut = new MySqlCommand("UPDATE urunler SET miktar = @yeniMiktar WHERE barkod_no = @barkod", baglan);
					guncelleKomut.Parameters.AddWithValue("@yeniMiktar", yeniMiktar);
					guncelleKomut.Parameters.AddWithValue("@barkod", barkodNo);
					guncelleKomut.ExecuteNonQuery();

					MessageBox.Show("Stok miktarı başarıyla güncellendi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				else
				{
					MessageBox.Show("Barkod numarasına ait ürün bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				baglan.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Stok güncellenirken bir hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
				baglan.Close();
			}
		}

        private void comboYeniMarka_SelectedIndexChanged(object sender, EventArgs e)
        {


            // Eğer kategori seçildi ise
            if (!string.IsNullOrEmpty(comboYeniKategori.Text))
            {
                try
                {
                    baglan.Open();

                    // Kategoriye ait markaları çekmek için SQL sorgusu
                    MySqlCommand komut = new MySqlCommand(
                        "SELECT marka_adi FROM markalar m " +
                        "INNER JOIN kategoriler k ON m.kategori_id = k.id " +
                        "WHERE k.kategori_adi = @kategori", baglan);
                    komut.Parameters.AddWithValue("@kategori", comboYeniKategori.Text);

                    MySqlDataReader oku = komut.ExecuteReader();

                    // Markaları comboYeniMarka'ya ekle
                    while (oku.Read())
                    {
                        comboYeniMarka.Items.Add(oku["marka_adi"].ToString());
                    }

                    oku.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Veri çekilirken hata oluştu: " + ex.Message);
                }
                finally
                {
                    baglan.Close();
                }
            }
        }

        private void comboYeniKategori_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            comboYeniMarka.Items.Clear(); // Önceki verileri temizle

            if (comboYeniKategori.SelectedIndex == -1) return; // Eğer kategori seçilmediyse işlem yapma

            try
            {
                // Veritabanı bağlantısını açıyoruz
                baglan.Open();

                // Kategorinin adını parametre olarak geçiyoruz
                MySqlCommand komut = new MySqlCommand(
                    "SELECT marka_adi FROM markalar m " +
                    "INNER JOIN kategoriler k ON m.kategori_id = k.id " +
                    "WHERE k.kategori_adi = @kategori", baglan);
                komut.Parameters.AddWithValue("@kategori", comboYeniKategori.Text);

                // Sorgu sonucu alınan markaları combobox'a ekliyoruz
                MySqlDataReader oku = komut.ExecuteReader();

                if (!oku.HasRows)
                {
                    MessageBox.Show("Bu kategoriye ait markalar bulunamadı.");
                }

                // Markaları comboYeniMarka'ya ekliyoruz
                while (oku.Read())
                {
                    comboYeniMarka.Items.Add(oku["marka_adi"].ToString());
                }

                oku.Close(); // Okuma tamamlandığında Reader'ı kapatıyoruz
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veri çekilirken hata oluştu: " + ex.Message);
            }
            finally
            {
                baglan.Close(); // Bağlantıyı her durumda kapatıyoruz
            }

        }

        private void txtMevcutBarkod_TextChanged(object sender, EventArgs e)
        {
            string barkodNo = txtMevcutBarkod.Text.Trim();

            if (!string.IsNullOrEmpty(barkodNo))
            {
                BarkodBilgileriniGetir(barkodNo);
            }
        }

        private void btnYeniTemizle_Click(object sender, EventArgs e)
        {
            foreach (Control item in groupBox1.Controls)
            {
                if (item is TextBox || item is ComboBox)
                    item.Text = ""; // TextBox ve ComboBox kontrollerinin içeriğini temizle
            }

            comboYeniKategori.SelectedIndex = -1;  
            comboYeniKategori.Items.Clear();
            comboYeniMarka.SelectedIndex = -1;  
            comboYeniMarka.Items.Clear();
        }

        private void btnMevutTemizle_Click(object sender, EventArgs e)
        {
            foreach (Control item in groupBox2.Controls)
            {
                if (item is TextBox || item is ComboBox)
                    item.Text = ""; // TextBox ve ComboBox kontrollerinin içeriğini temizle
            }
        }
    }
}
