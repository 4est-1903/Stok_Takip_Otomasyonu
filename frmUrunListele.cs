using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace StokTakipOtomasyonu
{
    public partial class frmUrunListele : Form
    {
        public frmUrunListele()
        {
            InitializeComponent();
        }

        MySqlConnection baglan = new MySqlConnection("Server=localhost;Database=stok_takip;Uid=root;Pwd=root;");
        DataSet ds = new DataSet();

        private void frmUrunListele_Load(object sender, EventArgs e)
        {
            UrunListele();
            KategoriGetir();
        }

        private void UrunListele(string kategori = "", string marka = "")
        {
            ds.Clear();
            string query = "SELECT * FROM urunler";
            bool whereAdded = false;

            if (!string.IsNullOrEmpty(kategori))
            {
                query += " WHERE kategori = @kategori";
                whereAdded = true;
            }

            if (!string.IsNullOrEmpty(marka))
            {
                if (whereAdded)
                {
                    query += " AND marka = @marka";
                }
                else
                {
                    query += " WHERE marka = @marka";
                }
            }

            baglan.Open();
            MySqlDataAdapter adtr = new MySqlDataAdapter(query, baglan);
            if (!string.IsNullOrEmpty(kategori))
            {
                adtr.SelectCommand.Parameters.AddWithValue("@kategori", kategori);
            }
            if (!string.IsNullOrEmpty(marka))
            {
                adtr.SelectCommand.Parameters.AddWithValue("@marka", marka);
            }
            adtr.Fill(ds, "urunler");
            dataGridView1.DataSource = ds.Tables["urunler"];
            baglan.Close();
        }

        private void KategoriGetir()
        {
            baglan.Open();
            MySqlCommand komut = new MySqlCommand("SELECT * FROM kategoriler", baglan);
            MySqlDataReader oku = komut.ExecuteReader();
            while (oku.Read())
            {
                comboKategori.Items.Add(oku["kategori_adi"].ToString());
            }
            oku.Close();
            baglan.Close();
        }

        private void MarkaGetir(string kategoriAdi)
        {
            baglan.Open();
            MySqlCommand komut = new MySqlCommand("SELECT marka_adi FROM markalar WHERE kategori_id = (SELECT id FROM kategoriler WHERE kategori_adi = @kategoriAdi)", baglan);
            komut.Parameters.AddWithValue("@kategoriAdi", kategoriAdi);
            MySqlDataReader oku = komut.ExecuteReader();

            while (oku.Read())
            {
                comboMarka.Items.Add(oku["marka_adi"].ToString());  // Marka adı ekleniyor
            }
            oku.Close();
            baglan.Close();
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            txtMevcutBarkod.Text = dataGridView1.CurrentRow.Cells["barkod_no"].Value.ToString();
            txtMevcutKategori.Text = dataGridView1.CurrentRow.Cells["kategori"].Value.ToString();
            txtMevcutMarka.Text = dataGridView1.CurrentRow.Cells["marka"].Value.ToString();
            txtMevcutUrunAdi.Text = dataGridView1.CurrentRow.Cells["urun_adi"].Value.ToString();
            txtMevcutMiktar.Text = dataGridView1.CurrentRow.Cells["miktar"].Value.ToString();
            txtMevcutAlis.Text = dataGridView1.CurrentRow.Cells["alisfiyat"].Value.ToString();
            txtMevcutSatis.Text = dataGridView1.CurrentRow.Cells["satisfiyat"].Value.ToString();
            txtAltLimit.Text = dataGridView1.CurrentRow.Cells["alt_limit"].Value.ToString();
        }

        private void btnGuncelle_Click(object sender, EventArgs e)
        {
            try
            {
                baglan.Open();
                MySqlCommand komut = new MySqlCommand("UPDATE urunler SET urun_adi=@urunad, miktar=@miktar, alisfiyat=@alisfiyat, satisfiyat=@satisfiyat, alt_limit=@altlimit WHERE barkod_no=@barkod", baglan);
                komut.Parameters.AddWithValue("@barkod", txtMevcutBarkod.Text);
                komut.Parameters.AddWithValue("@urunad", txtMevcutUrunAdi.Text);
                komut.Parameters.AddWithValue("@miktar", int.Parse(txtMevcutMiktar.Text));
                komut.Parameters.AddWithValue("@alisfiyat", double.Parse(txtMevcutAlis.Text));
                komut.Parameters.AddWithValue("@satisfiyat", double.Parse(txtMevcutSatis.Text));
                komut.Parameters.AddWithValue("@altlimit", int.Parse(txtAltLimit.Text));
                komut.ExecuteNonQuery();
                baglan.Close();
                MessageBox.Show("Ürün güncellendi!");
                ds.Tables["urunler"].Clear();
                UrunListele();
                Temizle();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
                baglan.Close();
            }
        }

        private void btnSil_Click(object sender, EventArgs e)
        {
            try
            {
                baglan.Open();
                MySqlCommand komut = new MySqlCommand("DELETE FROM urunler WHERE barkod_no=@barkod", baglan);
                komut.Parameters.AddWithValue("@barkod", dataGridView1.CurrentRow.Cells["barkod_no"].Value.ToString());
                komut.ExecuteNonQuery();
                baglan.Close();
                MessageBox.Show("Ürün silindi!");
                ds.Tables["urunler"].Clear();
                UrunListele();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
                baglan.Close();
            }
        }

        private void Temizle()
        {
            foreach (Control item in this.Controls)
            {
                if (item is TextBox)
                    item.Text = "";
            }
        }

        private void txtBarkodAra_TextChanged(object sender, EventArgs e)
        {
            DataTable tablo = new DataTable();
            baglan.Open();
            MySqlDataAdapter adtr = new MySqlDataAdapter("SELECT * FROM urunler WHERE barkod_no LIKE @barkod", baglan);
            adtr.SelectCommand.Parameters.AddWithValue("@barkod", "%" + txtBarkodAra.Text + "%");
            adtr.Fill(tablo);
            dataGridView1.DataSource = tablo;
            baglan.Close();
        }

        private void comboKategori_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboKategori.SelectedItem != null)
            {
                string selectedKategori = comboKategori.SelectedItem.ToString();

                // Önce marka combobox'ını temizle
                comboMarka.Items.Clear();

                // Seçilen kategoriye göre markaları getir
                MarkaGetir(selectedKategori);

                // Ürünleri kategoriye göre filtrele
                UrunListele(selectedKategori);
            }
            else
            {
                // Kategori seçilmediyse
                comboMarka.Items.Clear();  // Marka ComboBox'ını temizle
                comboMarka.SelectedIndex = -1;  // Seçimi temizle
                UrunListele();  // Filtreyi kaldır ve tüm ürünleri listele
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UrunListele();
        }

        private void comboMarka_SelectedIndexChanged(object sender, EventArgs e)
        {

            string selectedKategori = comboKategori.SelectedItem.ToString();
            string selectedMarka = comboMarka.SelectedItem != null ? comboMarka.SelectedItem.ToString() : "";

            UrunListele(selectedKategori, selectedMarka);

        }

        private void btnTemizle_Click(object sender, EventArgs e)
        {
            comboKategori.SelectedIndex = -1;  
            comboMarka.Items.Clear();         
            comboMarka.SelectedIndex = -1; 
            UrunListele();
            Temizle();
        }
    }
    }

